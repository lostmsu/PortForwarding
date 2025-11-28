#requires -Modules au
Import-Module au

$releases = 'https://github.com/lostmsu/PortForwarding/releases/latest'

function global:au_GetLatest {
	$release = Get-GitHubRelease -Owner 'lostmsu' -Repository 'PortForwarding' -Latest
	$version = $release.tag_name.TrimStart('v')
	$asset = $release.assets | Where-Object { $_.name -ieq 'PortOpen.exe' } | Select-Object -First 1

	if (-not $asset) {
		throw "PortOpen.exe asset not found in latest release $($release.tag_name)"
	}

	return @{
		Version = $version
		URL32   = $asset.browser_download_url
	}
}

function global:au_BeforeUpdate($Package) {
	# Fetch checksum for the current release asset
	$Package.Checksum32 = Get-RemoteChecksum $Package.URL32 -Algorithm 'sha256'
}

function global:au_SearchReplace {
	return @{
		'tools\chocolateyinstall.ps1' = @{
			'(^\$url\s*=\s*)\'.*?\''      = "`$1'$($Latest.URL32)'"
			'(^\$checksum\s*=\s*)\'.*?\'' = "`$1'$($Latest.Checksum32)'"
		}
		'portopen.nuspec' = @{
			'(<version>)[^<]+(</version>)'         = "`$1$($Latest.Version)`$2"
			'(<releaseNotes>).*?(</releaseNotes>)' = "`$1PortOpen $($Latest.Version) packaged from GitHub release.`$2"
		}
		'legal\VERIFICATION.txt' = @{
			'(releases/download/)[^/]+(/PortOpen\.exe)' = "`$1v$($Latest.Version)`$2"
			'(Checksum:\s*)([A-Fa-f0-9]+)'              = "`$1$($Latest.Checksum32)"
		}
	}
}

Update-AUPackage -ChecksumFor 32
