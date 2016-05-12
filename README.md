# ActivityOverlay
WPF activity inticator

This control provides basic UI to display background activity:
  * "loading spinner"
  * cancel button (hidden by default)
  * UI to display information about exception and "retry" button
  * UI to display that task is done (hidden by default)
  
![alt tag](https://cloud.githubusercontent.com/assets/1528799/15213430/ce7e2890-184f-11e6-9eb4-aab606a4a75b.gif)

# How to use
  Step 1) Add namespace xmlns:a="clr-namespace:ActivityOverlay;assembly=ActivityOverlay" to XAML
  ```
  <Window
	...
	xmlns:a="clr-namespace:ActivityOverlay;assembly=ActivityOverlay" 
	...
  ```
  Step 2) Add ActivityControl to XAML
  ```
  <a:ActivityControl x:Name="Activity" HorizontalAlignment="Stretch" VerticalAlignment="Stretch">
  ```
  Step 3) Use Activity.EnqueueActivity method to run tasks
  ```
  Activity.EnqueueActivity(async (t) => { 
    var result = await myService.DoJobAsync();
    DataContext = result;
  }, "Getting results");
  ```

# License

    Copyright 2016 Volodymyr Baydalka

    Licensed under the Apache License, Version 2.0 (the "License");
    you may not use this file except in compliance with the License.
    You may obtain a copy of the License at

       http://www.apache.org/licenses/LICENSE-2.0

    Unless required by applicable law or agreed to in writing, software
    distributed under the License is distributed on an "AS IS" BASIS,
    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
    See the License for the specific language governing permissions and
    limitations under the License.
