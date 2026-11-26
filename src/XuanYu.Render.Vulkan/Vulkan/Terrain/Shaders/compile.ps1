$ErrorActionPreference = 'Stop'
$compiler = Join-Path $env:VULKAN_SDK 'Bin/glslc.exe'
$validator = Join-Path $env:VULKAN_SDK 'Bin/spirv-val.exe'
if (!(Test-Path -LiteralPath $compiler) -or !(Test-Path -LiteralPath $validator)) {
    throw 'VULKAN_SDK 必须指向包含 glslc 和 spirv-val 的现有 Vulkan SDK。'
}
$sources = @((Join-Path $PSScriptRoot 'terrain.vert'), (Join-Path $PSScriptRoot 'terrain.frag'),
    (Join-Path $PSScriptRoot '../Diagnostic/Shaders/terrain_probe.comp'))
foreach ($source in $sources) {
    $output = "$source.spv"
    & $compiler --target-env=vulkan1.3 -O $source -o $output
    if ($LASTEXITCODE -ne 0) { throw "shader 编译失败：$source" }
    & $validator --target-env vulkan1.3 $output
    if ($LASTEXITCODE -ne 0) { throw "SPIR-V 验证失败：$output" }
}
