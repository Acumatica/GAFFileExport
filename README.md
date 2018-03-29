# GAFFileExport
GAF File generation for Malaysian tax reporting to government 

Publication of this customization allows you to export Malaysian GST GAF audit file from standard Acumatica designed and developed Acumatica team.
Package contains extension library project and customization project with new screen, table and sitemap node.
To apply customization project you need to compile extension library to get a DLL and publish customization project.

Supported Versions: Acumatica 6.1, Acumatica 2017R2, Acumatica 2018R1.
Please note that this customization has been developed based on Malaysian government regulations from 2014 (GUIDE TO ENHANCE YOUR ACCOUNTING SOFTWARE TO BE GST COMPLIANT As at 11 SEPTEMBER 2014)

To publish customization do following:
1) Download or Fork project from GitHub to local folder on your computer.
2) Go to Customization Project (SM204505) screen
3) Create new Project with name GAFTaxesGAFExport
4) Open customization project created
5) On the customization project browser click "Source Control -> Open Project from Folder..."
6) Choose "<local project folder>\CustomizationProject" where you have downloaded sources
7) Make sure that new screen, table and sitemap note have appeared.
8) Open "<local project folder>\Sources\GAFTaxesGAFExport.sln" solution with Visual Studio 2012, 2015 or 2017
9) Link solution with your local Acumatica - you can read more here: http://asiablog.acumatica.com/2017/10/move-customization-project-to-source.html
  9.1) Replace website in the solution with local web site
  9.2) Update references of TaxesGAFExport project with PX.Common.dll, PX.Data.dll and PX.Objects.dll from your local Acumatica Instance. 
  9.3) Add reference to the TaxesGAFExport project from your local Acumatica instance
10) Compile TaxesGAFExport project and make sure that TaxesGAFExport.dll appears in Site/bin folder.
11) Go to Customization project, refresh the page and add TaxesGAFExport.dll to Files section.
12) Click "Publish -> Publish Customization Project (Ctrl+Space)"
