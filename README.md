# frejaAdfsProvider
 A simplistic ADFS Provider for Freja eID and Freja Org ID

### Be warned

This code is to be considered experimental. The code is running but in derrived and changed form in a number of projects, major
difference is that all the communication logic is moved out into the dependency of [eid-provider-net](https://github.com/DSorlov/eid-provider-net) which is a .net 5.0 library
in prepparation to make this even more versatile. From the start the code is adapted from examples and much of the structure is taken from [adfsmfaadapter](https://github.com/tinodo/adfsmfaadapter)
 
### Crude instructions

1. Get the Microsoft.IdentityServer.Web.dll

Copy Microsoft.IdentityServer.Web.dll from the AD FS directory (C:\Windows\ADFS) to your developer machine.
Create a new reference to the DLL in the project and set the "Copy Local" property to "False".

2. Build

3. Get the PublicKeyToken

To get the PublicKeyToken for the Authentication Provider, run this command from a Visual Studio Command Prompt:

SN -T "C:\PATHTOFILE\FrejaADFSProvider.dll"

Copy the "Public key token" from the output;

Microsoft (R) .NET Framework Strong Name Utility  Version 4.0.30319.0
Copyright (c) Microsoft Corporation.  All rights reserved.

Public key token is XXXXXXXXXXXXXXXX

4. Copy the Authentication Provider DLL to the AD FS server, for example in "C:\Program Files (x86)\FrejaADFSProvider"

5. Register the Authentication Provider in the GAC

From a PowerShell Window with elevated privileges, issue these commands on each AD FS Server in the Farm:

Set-location "C:\Program Files (x86)\FrejaADFSProvider"
[System.Reflection.Assembly]::Load("System.EnterpriseServices, Version=4.0.0.0, Culture=neutral, PublicKeyToken=1ab23c0000dd1234")
$publish = New-Object System.EnterpriseServices.Internal.Publish
$publish.GacInstall("C:\Program Files (x86)\FrejaADFSProvider\TOTPAuthenticationProvider.dll")

Replace the PublicKeyToken with your Public Key Token and replace the location and name of the file if you need to.

8. Register the Authentication Provider with AD FS

From a PowerShell Window with elevated privileges, issue these commands:

$typeName = "com.sorlov.frejaadfsprovider.AuthenticationAdapter, FrejaAdfsProvider, Version=1.0.0.0, Culture=neutral, PublicKeyToken=XXXXXXXXXXXX"
Register-AdfsAuthenticationProvider -TypeName $typeName -Name "Freja eID" -Verbose

9. Now you can use the provider in AD FS!