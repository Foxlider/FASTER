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

***Build***  
[![Build](https://github.com/Foxlider/FASTER/actions/workflows/build.yml/badge.svg?branch=master)](https://github.com/Foxlider/FASTER/actions/workflows/build.yml)
[![CodeQL](https://github.com/Foxlider/FASTER/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/Foxlider/FASTER/actions/workflows/codeql-analysis.yml)
[![Azure Build Status](https://dev.azure.com/keelah/FASTER/_apis/build/status/Faster%20Release%20Builder?branchName=master)](https://dev.azure.com/keelah/FASTER/_build/latest?definitionId=8&branchName=master)


[![Discord](https://img.shields.io/discord/366955806777671681?label=Discord&logo=discord&logoColor=white&style=for-the-badge)](https://discord.gg/SJxnTNuNJN)

#### **INTRO**

FASTER is a Windows tool for installing, updating and running Arma 3 dedicated servers. It manages the server files, Workshop mods, multiple server profiles and their config files from one place.

FASTER is an extensive rewrite of FAST2 and FAST. There was no update for a long time and it was written in VB. I translated the whole project to C# using .NET 10.0.  
Thanks go out to all the guys who helped the development and those who will test it. Also, to BI for giving us an awesome game to play with and break.

*This tool is in no way affiliated with Bohemia Interactive or its developers.*


##### **PREREQUISITES**

- Windows 10 or later or Windows Server 2012 or later, 64-bit. FASTER must be run as administrator.
- Steam account with valid copy of Arma 3. A separate account just for your server is a good idea: FASTER stores the password (encrypted with Windows DPAPI for your Windows user) and you may be asked for a Steam Guard code on first login.
- Basic understanding of Arma 3 dedicated servers.
- No separate .NET install is needed. The release is self-contained.


##### **_FEATURES_**

- General Features
  - Theming System and Metro UI
  - Easy to read and share config files
  - Built-in self-updater (Settings > Update FASTER)
  - Optional debug log with automatic rotation

- Steam Automation
  - Install and update Arma 3 Server (Stable, Profiling, DLCs)
  - Supported server DLCs: Contact, GM, S.O.G. Prairie Fire, CSLA, Western Sahara, Spearhead 1944, Reaction Forces, Expeditionary Forces
  - Add and update Arma 3 Workshop mods by ID or URL
  - Import mod presets from Arma 3 Launcher
  - Check for Steam mod updates
  - Supports Steam Guard and Mobile Auth

- Mod Management
  - All mods are kept in one Mod Staging Directory
  - Deploy mods to your Arma 3 server folder as links, without copying files
  - Add local mods from any folder (linked, not copied)
  - Purge and reinstall mods, or remove mods that no server profile uses

- Multiple Server Profiles
  - Save and load multiple server presets
  - Supports most server config options, plus a free-text field for any extra command line arguments
  - Custom mission params
  - Custom difficulty
  - Headless Client support and auto launch
  - Correctly displays mods in Server Browser
  - Load Steam Mod Presets (html presets) to your profiles
  - Manually editable config files

- Server Status
  - Live CPU and memory gauges
  - Per-server CPU and memory graphs for running arma3server processes

##### **_ISSUES and FEEDBACK_**

As always, best place to report issues is on the [GitHub Repo](https://github.com/Foxlider/FASTER/issues). As for general discussion I'll keep an eye on the BI forum thread but I'll be more active on [Discord](https://discord.gg/SJxnTNuNJN).


##### **_DOCUMENTATION_**

A complete Documentation is available on the [GitHub Wiki](https://github.com/Foxlider/FASTER/wiki)


##### **_SCREENSHOTS_**
<details>
  <summary>Screenshots below</summary> 

  Main Menu
  ![MainMenu](https://imgur.com/Izf9Kmm.png)

  Profile Mods Menu
  ![Profile Mods Menu](https://imgur.com/D8LqSO6.png)

  Mods Menu
  ![Mods Menu](https://imgur.com/VivCLM3.png)

  Server Menu
  ![Profile Menu](https://imgur.com/1GuLqu2.png)

</details>

##### **_SOCIAL_**  
X (Twitter) :
[@FoxliderAtom](https://x.com/FoxliderAtom)
[![Twitter Follow](https://img.shields.io/twitter/follow/FoxliderAtom.svg?label=Follow&logo=twitter&style=for-the-badge)](https://twitter.com/FoxliderAtom)

Bohemia Interactive Forums :  
[Fox's Arma Server Tool Extended Rewrite (FASTER)](https://forums.bohemia.net/forums/topic/224359-foxs-arma-server-tool-extended-rewrite-faster/)

##### **_SUPPORT_**
Support the dev by making a donation here :
[![Donate](https://img.shields.io/badge/Donate-PayPal-blue.svg?style=for-the-badge&logo=paypal)](https://www.paypal.com/cgi-bin/webscr?cmd=_s-xclick&hosted_button_id=49H6MZNFUJYWA)
  
