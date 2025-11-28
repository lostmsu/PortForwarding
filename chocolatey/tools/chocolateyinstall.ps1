$ErrorActionPreference = 'Stop'

$packageName = 'portopen'
$toolsDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$exeName = 'PortOpen.exe'
$url = 'https://github.com/lostmsu/PortForwarding/releases/download/v0.4.2/PortOpen.exe'
$checksum = '2B3CA94616214713F217FD1D1B92952D6F78B934616417E79452C1862D12D7E2'

$exePath = Join-Path $toolsDir $exeName

Get-ChocolateyWebFile -PackageName $packageName -FileFullPath $exePath -Url $url -Checksum $checksum -ChecksumType 'sha256'
