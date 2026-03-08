# This Game is NP-Complete

An attempt to create a game in Unity that is NP-Complete. 
I was inspired by *LaserTank*, [which was proved to be NP-Complete](https://arxiv.org/pdf/1908.05966), despite the creator probably not intending to create a NP-Complete game.
As such, I wanted to create a game that is also NP-Complete despite not seemingly so.

I don't intend to provide the proof of NP-Completeness within the game; it shall be left as an exercise for the reader. ~~It would also make a good thesis for my CS degree, right?~~

## Running EditMode Tests (CLI)

Use Unity in batch mode and let the test runner exit on its own (do not pass `-quit`):

```powershell
$xml='Y:\Unity Projects\This Game is NP-Complete\Logs\EditModeResults.xml'
$log='Y:\Unity Projects\This Game is NP-Complete\Logs\EditMode.log'

if(Test-Path $xml){Remove-Item $xml -Force}
if(Test-Path $log){Remove-Item $log -Force}

$args='-batchmode -nographics -accept-apiupdate -projectPath "Y:\Unity Projects\This Game is NP-Complete" -runTests -testPlatform EditMode -testResults "'+$xml+'" -logFile "'+$log+'"'
$p=Start-Process -FilePath 'Y:\Unity Engines\6000.3.8f1\Editor\Unity.exe' -ArgumentList $args -Wait -PassThru
"EXIT_CODE=$($p.ExitCode)"
"XML_EXISTS=$(Test-Path $xml)"
```

EditMode tests are located in `Assets/Tests/EditMode/Editor`.
