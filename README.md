# Smarty-Cars - a short, critical examination
I have implemented two different types of AI in this project.

For **machine learning**, I chose **NEAT** ([Neuroevolution of augmenting topologies](https://en.wikipedia.org/wiki/Neuroevolution_of_augmenting_topologies)), since I deem it an exciting approach, and I am not the only one with that assessment - there is a fair amount of reading and examples. Machine learning was also clearly my focus: The underlying neural network and its evolution are self-constructed.

![Smarty-Cars NEAT](https://user-images.githubusercontent.com/18394014/92759209-c89fa500-f38f-11ea-803d-0c8b815c61d9.png)

When you consider that every new generation consists of 50 cars (however, the stupidest quickly disappear) and ECS ([Entity component system](https://en.wikipedia.org/wiki/Entity_component_system)) is not used (ECS is pretty much a work in progress at the time of this project), it runs very decent. In the beginning, when everyone's going crazy, I've just got around 30 FPS in the editor, but that's improving very fast (among other things by thinning) - with only a few survivors of a generation, it settles at about 80 FPS.

![Hardware used for testing](https://user-images.githubusercontent.com/18394014/68081588-a30d4b00-fe10-11e9-8a91-26d7d840781c.png)

I think all in all, measured by the significant number of (at least rather simple) calculations, this is a pretty good result. With reasonably correct settings (to obtain ideal values, you could spend quite some time of trial and error) success comes pretty fast. Unless you are vicious and prevent any success to the Smarty-Cars through mean physics settings, the best of them master a medium difficulty track after two (I witnessed this several times) to ten generations (beware: The car's center of mass has an immense influence on this rather variable value!). At first, I was surprised by how fast they learn, but that's exactly one of the strengths of NEAT. With more fine tuning, you could retrieve even better results, however, this would be very time consuming and in addition, quite stupid as well as laborious - my values should be quite all right, too.

---

Now is the time of the **steering behaviors**. I was amazed by how much more and better sources there are for car AI via machine learning. After this project, I would seriously consider using machine learning for the AI if I had to develop a racing game.

![Smarty-Cars Steering Behaviour](https://user-images.githubusercontent.com/18394014/92759234-cf2e1c80-f38f-11ea-9646-846164c359da.png)

Past extensive research, I came to the conclusion that it is certainly a reasonable choice to use a **system of traversable waypoints**. Many racing game developers choose this if they did not favour machine learning. For a more versatile solution, an **approach to avoid obstacles** had to be complemented. After further internet research, my choice fell on a **sensor system**. The needed raycasts nearly drove me crazy because they are almost unpredictable in certain situations (slow vehicles, for example). I'm not the type who gives up, so I just fought it out. Finally, I can state that I am quite happy with the result.

In terms of performance, this approach should also be a little less taxing. A direct performance comparison of the two AI methods is rather difficult with this project, since the "SmartCar" (script name), as opposed to his NEAT colleagues, drives alone on a rather easy circuit. Piloting the waypoints, certainly needs very little resources; the eight raycasts per vehicle (NEAT needs nine) are clearly more significant. Overall, I am very sure that this approach is generally more performant.

The vehicles are all physically controlled through Unity's [Wheel Colliders](https://docs.unity3d.com/Manual/class-WheelCollider.html).
