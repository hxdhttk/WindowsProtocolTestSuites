##################################################################################
## Copyright (c) Microsoft Corporation. All rights reserved.
##################################################################################

#############################################################################
##
## Microsoft Windows Powershell Scripting
## File:           Install-SelfSignedCert.ps1
## Purpose:        Install self signed certificate.
## Requirements:   Windows Powershell 2.0
## Supported OS:   Windows Server 2008 R2, Windows Server 2012, Windows Server 2012 R2,
##                 Windows Server 2016 and later
##
##############################################################################

Param
(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [String]$CertFile
)

certutil.exe -addstore MY $CertFile
certutil.exe -addstore root $CertFile