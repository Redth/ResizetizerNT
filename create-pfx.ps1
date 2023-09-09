# Author: Dmytro Bondarenko
Write-Output "This script will create a self-signed certificate for code signing."
# admin rights required
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) { 
    Write-Warning "You do not have Administrator rights to run this script!"
    Write-Warning "Wait for automatic elevation to kick in...."
    Start-Sleep -Seconds 1.5;
}
# elevate permissions
if (-not ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole] "Administrator")) { 
    Start-Process powershell.exe "-NoProfile -ExecutionPolicy Bypass -File `"$PSCommandPath`"" -Verb RunAs
    break
}
Write-Output "Creating PFX file..."
$password = Read-Host -Prompt "Enter the password for the PFX file:" -AsSecureString;

$cert = New-SelfSignedCertificate -Type CodeSigningCert -Subject "CN=ResizetizerNT.2023"
$pfxFile = "$PSScriptRoot\ResizetizerNT.2023.pfx";
Export-PfxCertificate -Cert $cert -FilePath $pfxFile -Password $password
Write-Output "PFX file created."

# write base64 to file
Write-Output "Converting PFX file to base64..."
$pfxContent = Get-Content -Path $pfxFile -Encoding Byte
[System.Convert]::ToBase64String($pfxContent) | Out-File -FilePath "$pfxFile.base64" -Encoding ascii
Write-Output "PFX file was converted to base64 and saved to file.";
Read-Host -Prompt "Press enter to exit..."

