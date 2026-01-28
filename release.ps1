$version = Read-Host "Enter version number (e.g., v1.0.5)"
gh release create $version ./build/MyInstaller.msi --title "Release $version" --generate-notes
