%one action occupancy: 
close all 
clc 
clear all 

%% initialization

nx=7;
ny=7;
nz=7;
g.x=0:nx-1;
g.y=0:ny-1;
g.z=0.5:nz-0.5;
d=[1 1 1];      %default length of side of voxel
c='g';          %default color of voxel
alpha=0.7;      %default transparency 

%%
occupance_for1=zeros(3,3,3);
occupance_for1(2,2,2)=1;
b=occupance_for1;
b(b>1)=1;
plotVoxels(b,g,d,'k',1)

occupance_for1=zeros(3,3,3);
occupance_for1(2,1,1)=1;
occupance_for1(1,2,1)=1;
occupance_for1(3,2,1)=1;
occupance_for1(2,3,1)=1;
b=occupance_for1;
b(b>1)=1;

plotVoxels(b,g,d,'b',alpha)
legend('voxel','neighbors')
view([41 76])

occupance_for1=zeros(3,3,3);
occupance_for1(1,1,2)=1;
occupance_for1(1,3,2)=1;
occupance_for1(3,1,2)=1;
occupance_for1(3,3,2)=1;

hold on 
b=occupance_for1;
b(b>1)=1;
plotVoxels(b,g,d,'g',alpha)

occupance_for1=zeros(3,3,3);
occupance_for1(2,1,3)=1;
occupance_for1(1,2,3)=1;
occupance_for1(3,2,3)=1;
occupance_for1(2,3,3)=1;

hold on 
b=occupance_for1;
b(b>1)=1;
plotVoxels(b,g,d,'r',alpha)
legend('voxel','neighbors at z=-1','neighbors at z=0','neighbors at z=1')
axis equal 
set(gcf, 'color','white','Position', [800 350 150 150]) % x y width height
set(gca, 'FontName', 'Times','Fontsize',5)
titl = sprintf('Roombots-neighborhood');
title(titl)
view([-40 40])
title(titl,'horizontalalignment','right')
export_fig ../../ICRA2019/figures/pdf/heuristic_RB1.pdf
%%
figure 
b=zeros(3,3,3);
b(2,2,2)=1;
plotVoxels(b,g,d,'k',1)
b=ones(3,3,3);
b(2,2,2)=0;
plotVoxels(b,g,d,'r',0.3)
legend('voxel','neighbors')
axis equal 
set(gcf, 'color','white','Position', [800 350 150 150]) % x y width height
set(gca, 'FontName', 'Times','Fontsize',5)
titl = sprintf('Roombots-neighborhood');
title(titl)
view([-40 40])
title(titl,'horizontalalignment','right')
export_fig ../../ICRA2019/figures/pdf/heuristic_8.pdf
%%
figure
hold on 
b=zeros(9,9,9);
b(1,1,1)=1;
plotVoxels(b,g,d,'k',1)
hold on 
occupance_for1=zeros(9,9,9);
occupance_for1(:,:,5)=...
[   0,0,0,0,1,0,0,0,0;
    0,1,0,0,0,0,0,1,0;
    0,0,1,0,1,0,1,0,0;
    0,0,0,1,0,1,0,0,0;
    1,0,1,0,0,0,1,0,1;
    0,0,0,1,0,1,0,0,0;
    0,0,1,0,1,0,1,0,0;
    0,1,0,0,0,0,0,1,0;
    0,0,0,0,1,0,0,0,0];

occupance_for1(:,:,6)=...
[   0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,1,0,1,0,0,0;
    0,0,1,0,1,0,1,0,0;
    0,0,0,1,0,1,0,0,0;
    0,0,1,0,1,0,1,0,0;
    0,0,0,1,0,1,0,0,0;
    0,1,0,0,0,0,0,1,0;
    0,0,0,0,1,0,0,0,0];

occupance_for1(:,:,7)=...
[   0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,1,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,1,0,1,0,1,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,1,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0];

occupance_for1(:,:,8)=...
[   0,0,0,0,1,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    1,0,0,0,0,0,0,0,1;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,1,0,0,0,0];
occupance_for1(:,:,9)=...
[   0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,1,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0;
    0,0,0,0,0,0,0,0,0];
occupance_for1(:,:,1)=occupance_for1(:,:,9);
occupance_for1(:,:,2)=occupance_for1(:,:,8);
occupance_for1(:,:,3)=occupance_for1(:,:,7);
occupance_for1(:,:,4)=occupance_for1(:,:,6);
b=occupance_for1(5:9,5:9,5:9);
b(1,1,1)=0;
plotVoxels(b,g,d,'b',0.7)

view([38,15]);

l=legend('voxel','neighbors');
set(l,'Position',[-0.198564566006435,0.873884776494247,0.398936170212766,0.116391752577328],'Units','normalized');
axis equal 
set(gcf, 'color','white','Position', [800 350 150 150]) % x y width height
set(gca, 'FontName', 'Times','Fontsize',5)
titl = sprintf('Roombots-neighborhood');
title(titl)
title(titl,'horizontalalignment','center')
export_fig ../../ICRA2019/figures/pdf/heuristic_RB2.pdf