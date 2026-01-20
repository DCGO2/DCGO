# Install Instruction

**Download the following assets:**

1. Unity Hub
2. GitHub Desktop
3. Git
4. DCGO (Specifically the Assets folder to make things easier later)

**Installing unity version:**

01. Open Unity Hub
02. Go to the Installs tab on the left
03. Click on Install Editor on top right
04. Go to Archives tab
05. Click on the download archives link to open it in an internet browser
06. Click on 2021 then go down till you find version 2021.3.45f2 and install it
07. If it asks you to open Unity Hub or any similar messages, select the affirmative option
08. Click on Continue without making any additional selections
09. Read and agree to terms and Install, continue instructions while it downloads and installs
10. You may close the internet browser and go back to Unity Hub
11. If a message has popped up asking you to Initialize Git LFS, click the affirmative option, otherwise wait for one to do so
12. Navigate to the repository location
13. Place the Assets folder next to the DCGO folder of the repository
14. This is a good opportunity to install Git (default settings work fine)
15. Go back to the Unity Hub and wait for it to prompt the next step
16. At some point the Unity Hub Install will prompt you to install Visual Studio Community 2019, do so unless you have already have a preffered IDE (If you do so, there's a possibility you might need the .NET desktop development and the Game development with Unity Workloads so select those as well and Install)
17. In the Unity Hub, click on the Projects tab on the left
18. Click on Add > Add project from disk on top right
19. Navigate to the repository location
20. Once you have the DCGO folder selected, click Open
21. Click on the newly appeared DCGO project to open it
22. Allow Unity Editor access to your Firewall if it asks for it
23. If any errors show and it asks you to Enter Safe Mode, click Ignore
24. Once it finishes, it might have a message about NormalMap settings, click Ignore
25. Click on Edit > Preferences > External tools
26. Make sure you have your preferred External Script Editor selected
27. It might require you to have certain checkmarks in the boxes so select all to be safe
28. Click on Regenerate project files to help your IDE identify coding issues later
29. Close preferences

**To start the game in the open project, follow the following instructions:**

01. Fix any errors as shown in the Console tab at the bottom left that can't be cleared by the Clear button in the same area, once you do, continue to next step
02. Go to the Project tab in the bottom left if not already there
03. Navigate to the Scenes Folder
04. Double click on Opening
05. Click on the Play button at the top center

# Use Intructions for

**New set process**
- Before cards are announced, issues will be created with each card ID
- A branch for the set will be created, all development should occur off that branch
- Each card will have dummy data created, this will need to be edited when the card is revealed (CardBaseEntity > [Set ID] > [Color] \ [Card Type])
- When set is ready for testing in beta new card data will be created in bulk before build is generated.
