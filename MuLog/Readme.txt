# 1. Directory di default (..\NuGetPackages)
.\Build-And-Pack.ps1 -IncrementVersion

# 2. Directory assoluta
.\Build-And-Pack.ps1 -IncrementVersion -OutputDir "C:\NuGetPackages"

# 3. Directory su disco diverso
.\Build-And-Pack.ps1 -OutputDir "D:\PacchettiNuGet"

# 4. Directory nella cartella del progetto
.\Build-And-Pack.ps1 -OutputDir ".\Packages"

# 5. Directory condivisa in rete
.\Build-And-Pack.ps1 -OutputDir "\\server\NuGetPackages"