param (
    [string]$Root = 'c:\Workspace\Source\Projects\tdc.game.chaosoverlord'
)

$reference = Join-Path $Root 'src\ChaosOverlords.Data\Reference'

function Update-Json {
    param (
        [string]$FilePath,
        [string]$ImagePath,
        [string]$ThumbnailPath
    )

    $raw = Get-Content $FilePath -Raw
    $entries = $raw | ConvertFrom-Json

    foreach ($entry in $entries) {
        if ($entry.PSObject.Properties.Name -notcontains 'Image') {
            $entry | Add-Member -NotePropertyName 'Image' -NotePropertyValue $ImagePath
        } else {
            $entry.Image = $ImagePath
        }

        if ($entry.PSObject.Properties.Name -notcontains 'Thumbnail') {
            $entry | Add-Member -NotePropertyName 'Thumbnail' -NotePropertyValue $ThumbnailPath
        } else {
            $entry.Thumbnail = $ThumbnailPath
        }
    }

    $entries | ConvertTo-Json -Depth 5 | Out-File $FilePath -Encoding utf8
}

Update-Json -FilePath (Join-Path $reference 'gangs.json') -ImagePath 'assets/dummy_gang.svg' -ThumbnailPath 'assets/dummy_gang_thumbnail.svg'
Update-Json -FilePath (Join-Path $reference 'items.json') -ImagePath 'assets/dummy_item.svg' -ThumbnailPath 'assets/dummy_item_thumbnail.svg'
Update-Json -FilePath (Join-Path $reference 'site.json') -ImagePath 'assets/dummy_site.svg' -ThumbnailPath 'assets/dummy_site_thumbnail.svg'
