# Hierarchical Pathfinding

In this project, we explore a variation of A* which utilizes the concept of abstracting a map into clusters and precomputing information to do pathfinding. This method is called [Near-Optimal Hierarchical Pathfinding (HPA*)](http://www.cs.ualberta.ca/~mmueller/ps/hpastar.pdf).

The idea is to separate a map into multiple levels of clusters and to precompute information on how to navigate between clusters so that we can use this information later to do pathfinding on a higher level. This method mainly addresses the issue of pathfinding in real-time. Searching for a path should be efficient and should be done often. Moreover, given that the state of a game is constantly changing, we often disregard many of the pathfinding done. By using HPA*, we divide a map into clusters and precompute information on how to travel from a cluster to another. Therefore, we can efficiently do pathfinding on a higher level at a much lower cost, reusing precomputed low level paths between clusters. Consequently, pathfinding is generally faster than a traditional A* algorithm, and disregarding unnecessary paths is less heartbreaking. However, given that we travel from cluster to cluster by predetermined boundary nodes, the resulting paths from this method can be a bit less optimal than a straightforward A*, hence the name “Near-Optimal”.

Near-Optimal Hierarchical Pathfinding was implemented on grid-based maps using arbitrarily-sized automatically generated clusters. We tested the results of our algorithm on real maps from BioWare’s commercial game Dragon Age: Origins, provided by the [Moving AI lab](http://movingai.com/) (See credits).


- - -

# Credits

* HPA* was introduced in [this paper](http://www.cs.ualberta.ca/~mmueller/ps/hpastar.pdf) written by Botea, Müller, and Schaeffer.

* All .map and .scen files found in the Maps folder come from Moving AI's [2d Pathfinding benchmark sets](https://movingai.com/benchmarks/grids.html)

* The Priority Queue used in this project comes from BlueRaja's [High Speed Priority Queue for C#](https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp).  All files in folder **Priority Queue** comes from the repo's **Priority Queue** folder.