# Fox's Arma Server Tool Extended Rewrite (FASTER)

#### Badges 
***GitHub***  
[![GitHub issues](https://img.shields.io/github/issues/Foxlider/FASTER.svg?logo=github&style=flat-square)](https://github.com/Foxlider/FASTER/issues)
![GitHub](https://img.shields.io/github/license/Foxlider/FASTER.svg?style=flat-square)
[![GitHub release](https://img.shields.io/github/release/Foxlider/FASTER.svg?logo=github&style=flat-square)](https://GitHub.com/Foxlider/FASTER/releases/)  
[![Github total downloads](https://img.shields.io/github/downloads/Foxlider/FASTER/total.svg?logo=github&style=flat-square)](https://GitHub.com/Foxlider/FASTER/releases/)
[![Github latest downloads](https://img.shields.io/github/downloads/Foxlider/FASTER/latest/total.svg?logo=github&style=flat-square)](https://GitHub.com/Foxlider/FASTER/releases/)
  
***Azure***  
[![Build Status](https://dev.azure.com/keelah/FASTER/_apis/build/status/Faster%20Release%20Builder?branchName=master)](https://dev.azure.com/keelah/FASTER/_build/latest?definitionId=8&branchName=master)
[![Build Status](https://vsrm.dev.azure.com/keelah/_apis/public/Release/badge/4b51eb35-4363-4038-8d99-543c01a3578f/2/2)](https://dev.azure.com/keelah/FASTER/_release)

***Code quality***  
[![Sonar Quality Gate](https://img.shields.io/sonar/quality_gate/Foxlider_FASTER?label=Code%20quality&logo=sonarcloud&logoColor=white&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/dashboard?id=Foxlider_FASTER)
[![Sonar Violations (long format)](https://img.shields.io/sonar/violations/Foxlider_FASTER?format=long&label=Issues&logo=sonarcloud&logoColor=white&server=https%3A%2F%2Fsonarcloud.io&style=flat-square)](https://sonarcloud.io/project/issues?id=Foxlider_FASTER&resolved=false)


[![Discord](https://img.shields.io/discord/366955806777671681?label=Discord&logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/2BUuZa3)

#### **INTRO**

FASTER is an extensive rewrite of FAST2 and FAST. There was no update for a long time and it was written in VB. I translated the whole project to C# using .NET Core 3.0.  
Thanks go out to all the guys who helped the developpment and those who will test it. Also, to BI for giving us an awesome game to play with and break.


##### **PREREQUISITES**

- Steam account with valid copy of Arma 3.
- Basic understanding of Arma 3 dedicated servers.


##### **_FEATURES_**

- General Features
  - Theming System and Metro UI
  - Easy to read and share config files
  - Automated Update process

- SteamCMD Automation
  - Install and update Arma 3 Server (Stable, Dev, DLCs, Legacy)
  - Install, update and manage Arma 3 Workshop mods
  - Import installed Steam Mods
  - Supports Steam Guard and Mobile Auth
  - Import mod presets from Arma 3 Launcher
  - Check for mod updates on app launch

- Multiple Server Profiles
  - Save and load multiple server presets
  - Supports all server config options
  - Supports all server command line options
  - Custom mission params
  - Custom difficulty
  - Headless Client support and auto launch
  - Correctly displays mods in Server Browser
  - Load Steam Mod Presets (html presets) to your profiles
  - Manually editable config files

- Local Mod Support
  - Reads local mods from server folder
  - Include additional folders to search


##### **_ISSUES and FEEDBACK_**

As always, best place to report issues is on the [GitHub Repo](https://github.com/Foxlider/FASTER/issues). As for general discussion I'll keep an eye on the BI forum thread but I'll be more active on [Discord](https://discord.gg/2BUuZa3).


##### **_DOCUMENTATION_**
  
A complete Documentation is available on the [GitHub Wiki](https://github.com/Foxlider/FASTER/wiki)


##### **_SCREENSHOTS_**
<details>
  <summary>Screenshots below</summary> 
   
  Main Menu 
  ![MainMenu](https://user-images.githubusercontent.com/19773387/147384230-3da8cb8d-4a3a-4ae2-90f4-3fc90a3954ad.png)
  
  Profile Mods Menu 
  ![Profile Mods Menu](https://private-user-images.githubusercontent.com/78136088/316247430-2488268d-9a55-44cc-9010-c306c5056a33.PNG?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3MTEyMDQwMTksIm5iZiI6MTcxMTIwMzcxOSwicGF0aCI6Ii83ODEzNjA4OC8zMTYyNDc0MzAtMjQ4ODI2OGQtOWE1NS00NGNjLTkwMTAtYzMwNmM1MDU2YTMzLlBORz9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNDAzMjMlMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjQwMzIzVDE0MjE1OVomWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTQxODRkOGZiOGNhN2E1ZDJkYjE5ODY1YzIyZTNiMmZjMGMyZjk4ZGZlMWYwYjMzM2IwMTVhN2YzNDUwYmQ5NDkmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0JmFjdG9yX2lkPTAma2V5X2lkPTAmcmVwb19pZD0wIn0.sum-WMePl76xam_FsJjLCys6c0Eo041wTrE2Hmjnm_M)
  
  Mods Menu 
  ![Steam Mods Menu](https://user-images.githubusercontent.com/19773387/147383857-ab63ab77-aeef-42ad-8b51-e0708096fbf9.png)
  
  Profile Menu 
  ![Profile Menu](https://user-images.githubusercontent.com/19773387/95013037-d0c0cc80-063d-11eb-8ca1-ba3f445e5461.png)
   
</details>
  
##### **_SOCIAL_**  
Twitter :
[@FoxliderAtom](https://twitter.com/FoxliderAtom)  
[![Twitter Follow](https://img.shields.io/twitter/follow/FoxliderAtom.svg?label=Follow&logo=twitter&style=for-the-badge)](https://twitter.com/FoxliderAtom)

Bohemia Interactive Forums :  
[Fox's Arma Server Tool Extended Rewrite (FASTER)](https://forums.bohemia.net/forums/topic/224359-foxs-arma-server-tool-extended-rewrite-faster/)

##### **_SUPPORT_**
Support the dev by making a donation here :
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg?style=for-the-badge&logo=paypal)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA)
  
