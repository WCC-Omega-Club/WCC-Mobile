# WCC-Mobile<br />
(Unofficial?) SUNY Westchester Community College Mobile App<br /><br /><br />

###Development Roadmap<br />

(ala truncated [Waterfall Model](https://en.wikipedia.org/wiki/Waterfall_model))<br />
***
####[x] ~~Concept/Requirements~~<br />

>~~Basic idea for the app. Description of _what the product should do_. Usually results in a Product Requirements Document.~~<br />

See: [Product Requirements](Development-Roadmap/Product-Requirements.md)<br /><br />

####[ ] Design ~~(We are here.)<br />
>Pre-Production: Planning, Graphic Mockups, & overall Software Architecture Outlining<br />

See: [Software Development Methodology]() 

The main **WCC Mobile** parent-app will take up a [Model-View-Controller (MVC)](https://en.wikipedia.org/wiki/Model%E2%80%93view%E2%80%93controller) software architecture pattern.<br />
The _Model_, the _View_, and the _Controller_ refer to three distinct _**groups of classes**_ whose functions and interactions make up the lifecycle of the application.<br /><br />

######A very brief run-down of typical MVC operation
The **User** looks at a GUI, whose output is specified by the **View**. The User will interact with the device depending on what they see, sending inputs to the **Controller**. The **Controller** acts as a combination human-to-machine translator, structure-to-structure mediator, and task scheduler. It derives the intent of the user's input, using its conclusions to demand changes to the **Model**. The **Model** encompasses the program's understanding/simulation of what's going on, and acts as a library for the majority of the stored data used by the other structures. When information has changed, it sends out an update notification. The **View**, who is constantly watching for these **Model** update notifications, detects the change, and adjusts its representation of the output GUI to reflect them. Then the cycle repeats.<br />
>_**Recall what's happened so far:**<br />
User (inputted) to >> Controller (demanded state change) from >> Model (updated internal state) and (notifies) >> View (adjusts output presentation) >> User (decides on new input to place) to >> Controller... [etc.]_

This modularity not only allows flexibility and future growth, but also makes it easier to see where the communication cycle has broken down when something has gone wrong.<br /><br />
***
>#####Development-Phase Progress<br />
>Current Goals:<br />
>[x] ~~Decide on an IDE, language, and optional add-in platforms~~ **(Microsoft Visual Studio, C#, Xamarin for Visual Studio)**<br />
>[x] ~~Decide on overall parent-app architecture pattern.~~ **(MVC)**<br />
>[ ] _**In-Progress**_ Begin sketching data structure relationships. Work out necessary information flow between parent-app and sub-apps. Do more research on mobile app design and common practices.<br />
>[ ] _**In-Progress**_ Ensure all developers have properly installed Visual Studio and Xamarin. Ensure all developers have played around with the tools, and created one or more simple sample applications. _Sample application should include a touch-event activated button that produces some desired result. (Due next meeting 4/11/16)_<br /><br />
>Optional/Helpful Goals:<br />
>[ ] Become familiar with Unified Modeling Language (UML2.x) diagram notation and convention. We will probably use this format to draw class diagrams and map their dependencies and relationships.<br />
>[ ] Write a brief _WCC-Mobile Programming Style Guide_. Having a consistent pattern of file trees, class member declarations, variable naming scheme, etc. will make it easier to read one another's code and know what to expect. (Some possibilities? http://stackoverflow.com/questions/4678178/style-guide-for-c).<br /><br />
>Future Goals:<br />
>[ ] Create small, sample/prototype implementation of MVC architecture. Test.<br />
>[ ] Continue sketching/outlining structural relationships, in ever-increasing definition.<br />
>[ ] "Finish" defining relationships between parent-app-MVC structure and sub-app structures. Ensure all information needed can theoretically be accessed by relevant structures. Of course, this is open to iteration and refactoring, as tests will tell.
***

<br />
####[ ] Implementation<br />
>Coding: Develop and integrate increasingly well-defined prototypes as the product takes shape
<br /><br />

####[ ] Verification<br />
>Thoroughly test software against as many normal-use and edge-use cases as possible. Fix code. Close bug reports.
<br /><br />

####[ ] Maintenance<br />
>Release the product. Create additions and patches to the software. Update the software to handle changes in supporting elements.
<br /><br />

