# decentdesigner
For creating and editing DecentSampler files

![image](https://user-images.githubusercontent.com/24799349/114380064-83d82880-9b81-11eb-81d2-d54c80ce0e67.png)

I made this in a week with no planning, so yes, the code is hot garbge (and uncommented, i had no intention of 'finishing' this), but it is technically functional, and if you wanted to make your own c# decentsampler editor this might have some helpful info!

I have no plans to continue supporting this project

I would reccomend reading about this a little bit if you want to use, it's not difficult it's just not entirely obvious how to do things or what things do sometimes
https://www.decentsamples.com/wp-content/uploads/2020/06/format-documentation.html

# HOW TO USE
- here is how to use this thing, there are some additional important notes at the bottom which you should read!!!!!

# SAMPLE LIST

![image](https://user-images.githubusercontent.com/24799349/114381475-11684800-9b83-11eb-85cd-19718f0948db.png)
- press 'import sample' to add a path to a sample
- click on the sample to select it.
- hold alt while hovering to preview the waveform
- the selected sample will be highlighted yellow, and will be the path used when creating a new tag
- double click a sample to change the path of the selected tag

# GROUP TAGS AND SAMPLE TAGS

![image](https://user-images.githubusercontent.com/24799349/114381745-61470f00-9b83-11eb-8f14-bd160753a370.png)
- press '+new group' to create a new group
- press '+new tag' to create a new sample tag
- hold alt while hovering to get more info about a group or tag
- you can minimise a group with the - button, or expand it with + button
- you can delete a group if it is empty by minimising it (x)
- you can remove a sample tag by clicking the '\-'
- the group shows how many sample tags are inside it in square brackets
- the selected group is highlighted in yellow, adding a new tag will add it to that group
- there must be a group to add a tag
- 
# SAMPLE VIEW, AND ATTRIBUTES

![image](https://user-images.githubusercontent.com/24799349/114382147-f0542700-9b83-11eb-8486-dc973b44d9d3.png)
- every group and sample tag has attributes
- you can add an attribute by choosing one from the dropdown and clicking the +, you can also put in a value which kinda works like a bad copy paste thing
- you can remove an attribute by clicking the '\-'
- select an attribute by clicking it, it will be highlighted yellow
- the sample view will work as a slider for the selected attribute, (start & end)
- scroll to zoom, middle click and drag to move the viewport
- you can set the start and duration of the viewport with the input boxes

# PIANO VIEW

![image](https://user-images.githubusercontent.com/24799349/114383392-6442ff00-9b85-11eb-8549-495be2024bc4.png)
- displays every sample tag in the selected group as a box, which represents the lowest and highest note to trigger that sample, and the lowest and highest velocity to trigger it
- you can resize the boxes for any edge or corner
- hold alt to get more information about the box you are hovering over
- moving the left & right edges does not change the rootNote, so be sure to do that yourself
- you may want to leave a gap of 1 velocity so that it doesn't play 2 samples at velocities where they overlap
- scroll to zoom, middle click and drag to move the viewport

# ADDITIONAL NOTES
## (important)
- you can only load XML files, so rename your .dspreset to .xml to load it
- and also it can only understand absolute sample paths I would reccomend using notepad++ to replace relative paths in the xml with absolute when you load, and change them back when you save
- if you load a file, and the sample tags don't appear, press ctrl+r to reload everything \- not sure why this bug exists, sorry.
- it gets very slow to resize the window when you have a complex file open, because i made it in unity because i am not a real programmer
