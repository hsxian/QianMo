del /s /q bin\realse\net8.0\linux-x64\SpatioTemporalCoexistence.WebApi-publish_linux
del /s /q bin\realse\net8.0\linux-x64\SpatioTemporalCoexistence.WebApi-publish_win

dotnet publish -r win-x64 --self-contained -p:PublishSingleFile=true -c realse -o SpatioTemporalCoexistence.WebApi-publish_win
pause
