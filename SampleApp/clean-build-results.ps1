# Get-ChildItem .\ -include bin,obj,bld,Backup,_UpgradeReport_Files,Debug,Release,ipch -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

# Get-ChildItem .\ -include bin,obj -Recurse | foreach ($_) { remove-item $_.fullname -Force -Recurse }

Get-ChildItem .\ -include bin,obj -Recurse -Force -ErrorVariable FailedItems -ErrorAction SilentlyContinue | foreach ($_) { remove-item $_.fullname -Force -Recurse }

$FailedItems | Foreach-Object {$_.CategoryInfo.TargetName}
