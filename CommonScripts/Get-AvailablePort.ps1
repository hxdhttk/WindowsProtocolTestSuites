#############################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
#############################################################

param
(
[int]$expectedPort = 50000
)

Function IsPortAvailable
{
    param(
    [int]$portNum,
    [string[]]$usedPorts
    )

    [bool]$isAvailable = $true

    foreach ($up in $usedPorts)
    {
        if ($up -match $portNum)
        {
            $isAvailable = $false
            break
        }
    }

    return $isAvailable
}

$usedPortsList = netstat -a | Select-String -AllMatches "\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}"

for([int]$port = $expectedPort; $port -le 65535; $port++)
{
    if (IsPortAvailable -portNum $port -usedPorts $usedPortsList) 
    {
        return $port
    }
}

Throw("No available port is found!")
