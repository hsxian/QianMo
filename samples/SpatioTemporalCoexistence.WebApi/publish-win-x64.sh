rm -rf publish_win7
dotnet publish -r win7-x64 --self-contained -p:PublishSingleFile=false -c realse -o publish_win7
datastr=`date "+%Y_%m_%d_%H_%M_%S"`
mv publish_win7 $datastr
7za a -t7z -r $datastr.7z $datastr/*
rm -rf $datastr