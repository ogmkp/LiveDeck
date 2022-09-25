<p align="center">
  <img width="200" src="logo_ld_livedeck_1.png">
</p>

<h1 align="center">LiveDeck</h1>

<p align="center">
  A web panel solution to control OBS-Studio
</p>

## About
<p align="center">
  LiveDeck is an open-source multi account web panel solution for OBS-Studio.<br />
  The LiveDeck web interface allows fast interaction between the panel and the service, including OBS-Studio.<br />
  Through the script in the config file, the panel can be customized exactly to your usage.<br /><br />
  <img src="ld_function.png">
</p>

 
## Features
 - Panel Functions
   - Change Source Visibility
   - Change Source Filter Visibility
   - Change Scene
   - Mute Audio Source
 - Service Properties
   - Function Config Script
     - Static
     - Switch
     - Invert
   - Websocket Reconnect
   - API Connection

## Notes
- Keep in mind that the services for the server currently only support Linux.
- Make sure that the services have the necessary read and write permissions and are located in the home directory of the server.
- The panel template has to be placed in the PanelService under the directory "templates" (Example: "/home/test/PanelService/templates/mypanel").
- I recommend to put the admin panel under a subdomain.

<br />
There are some markings that need to be adjusted.

Examples: <br />
- `[your domain]` = example.com <br />
- `[your sub domain]` = test.example.com <br />

## Dependencies

### livedeck-service

| **Dependency** | **Description** | **Version** | **License**
| -------------- | --------------- | ----------- | -----------
 | [Newtonsoft.Json@^13.0.1](https://www.nuget.org/packages/Newtonsoft.Json) | Json.NET is a popular high-performance JSON framework for .NET| 13.0.1 | MIT
 | [obs-websocket-dotnet@^4.9.1](https://www.nuget.org/packages/obs-websocket-dotnet) | Official .NET library (written in C#) to communicate with an obs-websocket server| 4.9.1 | MIT

### Request Interpreter

| **Dependency** | **Description** | **Version** | **License**
| -------------- | --------------- | ----------- | -----------
 | [obs-websocket-dotnet@^4.9.1](https://www.nuget.org/packages/obs-websocket-dotnet) | Official .NET library (written in C#) to communicate with an obs-websocket server| 4.9.1 | MIT

### Authorization Service

| **Dependency** | **Description** | **Version** | **License**
| -------------- | --------------- | ----------- | -----------
 | [Newtonsoft.Json@^13.0.2-beta1](https://www.nuget.org/packages/Newtonsoft.Json) | Json.NET is a popular high-performance JSON framework for .NET| 13.0.2-beta1 | MIT
 | [MongoDB.Driver@^2.17.1](https://www.nuget.org/packages/MongoDB.Driver) | Official .NET driver for MongoDB| 2.17.1 | Apache2
 | [NetRoad@^1.0.0](https://www.nuget.org/packages/NetRoad) | A lightweight network library written in C#| 1.0.0 | MIT
 | [Isopoh.Cryptography.Argon2@^1.1.12](https://www.nuget.org/packages/Isopoh.Cryptography.Argon2) | Argon2 Password Hasher written in C#| 1.1.12 | CC BY 4.0

### Panel Service

| **Dependency** | **Description** | **Version** | **License**
| -------------- | --------------- | ----------- | -----------
 | [Newtonsoft.Json@^13.0.2-beta1](https://www.nuget.org/packages/Newtonsoft.Json) | Json.NET is a popular high-performance JSON framework for .NET| 13.0.2-beta1 | MIT

## Contributors

<td align="center">
  <a href="https://github.com/peakstack">
    <img src="https://avatars.githubusercontent.com/u/43585195?size=128" width="100px;" alt=""/>
    <br />
    <sub>
      <b>peakstack</b>
    </sub>
  </a>
</td>
