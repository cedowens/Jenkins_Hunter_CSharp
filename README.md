# Jenkins_Hunter_CSharp
C# implementation of my original Jenkins Hunter script (orig in python). It uses threading to search for unauthenticated Jenkins instances on ports 8080, 80, and 443.

The script dumps all AD computers and checks those computers for unauth jenkins instances.

Results are returned to stdout - hosts with ports 80, 8080, or 443 open are returned as well as any hosts that appear to have unauthenticated jenkins running.

This can also be executed in memory remotely using Cobalt Strike's execute-assembly function.

Usage:
.\JenkinsHunter.exe
