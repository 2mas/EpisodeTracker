OpenCover.Console.exe -target:"C:/Program Files/dotnet/dotnet.exe" -targetargs:"test \"EpisodeTracker.Tests\EpisodeTracker.Tests.csproj\" --configuration Debug --no-build" -filter:"+[*]* -[*.Tests*]*" -oldStyle -register:user -output:"EpisodeTracker_OpenCover.xml"
codecov -f "EpisodeTracker_OpenCover.xml" -t <token>
