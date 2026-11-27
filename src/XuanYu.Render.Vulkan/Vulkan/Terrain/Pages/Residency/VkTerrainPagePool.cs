using Silk.NET.Vulkan;
using XuanYu.Render;

namespace XuanYu.Render.Vulkan;

// Render-thread owner: change protection/reuse slots only after the previous draw fence completes.
public sealed partial class VkTerrainPagePool : IDisposable
{
    private readonly VkTerrainPageArray _image;
    private readonly VkDemTexture _waterTexture;
    private readonly RenderWaterAtlas? _waterSource;
    private readonly VkTerrainPageTransfer _transfer;
    private readonly VkTerrainPoolBindings _bindings;
    private readonly Dictionary<long, int> _slots = [];
    private readonly HashSet<long> _pending = [];
    private readonly bool[] _used;
    private HashSet<long> _protected = [];
    private readonly int _limit;
    public int Capacity => _used.Length;
    public int AvailableSlots => Capacity - _slots.Count;
    public int UploadLimit => _limit;
    public int ResidentCount => _slots.Count - _pending.Count;
    public int PendingCount => _pending.Count;
    public int UploadBatches { get; private set; }
    public int UploadedPages { get; private set; }
    public ulong HeightBytes => _image.AllocatedBytes;

    internal VkTerrainPagePool(Vk vk, PhysicalDevice gpu, Device device, Queue queue, uint family,
        DescriptorSetLayout layout, int capacity, int limit, ulong budget, RenderWaterAtlas? water = null)
    {
        _waterSource = water;
        _limit = limit; _used = new bool[capacity];
        _image = new VkTerrainPageArray(vk, gpu, device, capacity, budget);
        try
        {
            _waterTexture = water is null ? VkDemTexture.Fallback(vk, gpu, device, queue, family)
                : VkDemTexture.FromWaterAtlas(vk, gpu, device, queue, family, water);
            try
            {
                _bindings = new VkTerrainPoolBindings(vk, gpu, device, layout, _image, capacity, _waterTexture);
                try { _transfer = new VkTerrainPageTransfer(vk, gpu, device, queue, family, limit); }
                catch { _bindings.Dispose(); throw; }
            }
            catch { _waterTexture.Dispose(); throw; }
        }
        catch { _image.Dispose(); throw; }
    }

    public void Protect(IEnumerable<long> keys) => _protected = keys.ToHashSet();
    public bool Contains(long key) => _slots.ContainsKey(key);
    public void EvictUnprotected()
    {
        foreach (long key in _slots.Keys.ToArray()) TryEvict(key);
    }

    public bool IsReady(long key) => _slots.ContainsKey(key) && !_pending.Contains(key);

    public bool TryEvict(long key) => !_protected.Contains(key) && !_pending.Contains(key) && _slots.Remove(key);

    public int Submit(IReadOnlyList<TerrainPoolUpload> requests)
    {
        if (_pending.Count != 0) return 0;
        int[] free = Enumerable.Range(0, _used.Length).Except(_slots.Values).Take(_limit).ToArray();
        var batch = requests.Where(p => !_slots.ContainsKey(p.Key)).Take(free.Length).ToArray();
        if (batch.Length == 0) return 0;
        var copies = batch.Select((p, i) => (free[i], _used[free[i]], p.Heights)).ToArray();
        _transfer.Submit(_image.Handle, copies);
        for (int i = 0; i < batch.Length; i++)
        {
            _slots.Add(batch[i].Key, free[i]);
            _pending.Add(batch[i].Key);
            _used[free[i]] = true;
        }
        UploadBatches++; UploadedPages += batch.Length;
        return batch.Length;
    }

    public bool Poll()
    {
        if (!_transfer.Poll()) return false;
        _pending.Clear();
        return true;
    }

    public void WaitForUpload()
    {
        _transfer.Wait();
        Poll();
    }

    public void Dispose()
    {
        try { _transfer.Dispose(); }
        finally { _bindings.Dispose(); _waterTexture.Dispose(); _image.Dispose(); }
    }
}