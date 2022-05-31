% Check Collision: 
clc;
clear all; 
close all;
ee2=[1,1];
% Setting fonts and lines 
set(groot, 'DefaultAxesFontSize',12);
set(groot, 'DefaultLineLineWidth',1);
%vexel options 
g.x=[0,1,2];
g.y=[0,1,2];
g.z=[1/2,1.5,2.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2

acm_fixed=0;
show_fig=1;
different_orientations=[0 0 0 ;
                        0  pi/2 0 ;
                        -pi/2 0 pi/2;
                        0 -pi/2 0;
                        pi/2 0 -pi/2;
                        pi 0 -pi];%[z,x,y,-x,-y,-z]
         
init_point=[1 1 1;
            0.5,1,1.5;
            1,0.5,1.5;
            1.5,1,1.5;
            1,1.5,1.5;
            1 1 2];
            
yaw=[0,pi/2,pi,3*pi/2];                    
fileID = fopen(sprintf('Voxel_Occupied_M1_%d.txt',acm_fixed), 'w');
for or=1:1
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ');

for y=1:1
    
    init_orientation=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
    %initialiaztion
    %init_orientation=[0 -1 0 0; 0 0 1 0; -1 0 0 0; 0 0 0 1];
    init_orientation(1:3,4)=init_point(or,:);
    
    b=zeros(3,3,3);
    roombot = init_roombot_module(acm_fixed,init_orientation,ee2);
    motors=[0,2*pi/3,-2*pi/3];

    for m=1:length(motors)
        b=zeros(3,3,3);
        m0=motors(m);
        if(acm_fixed==0)
            m1=-m0;
        else
            m1=m0;
        end 
        %Initial position.
        config=roombot.homeConfiguration;
        config(1).JointPosition=m1;
        config(2).JointPosition=0;
        config(3).JointPosition=0;
        tform = getTransform(roombot,config,'body6');
        EEtform = getTransform(roombot,config,'EndEffector');
        EE_pose=(EEtform(1:3,4));

        cell_ee=findvoxel(EEtform,[2,2,2]);
        b(cell_ee(1),cell_ee(2),cell_ee(3))=1;
        b(cell_ee(1),cell_ee(2),cell_ee(3)-1)=1;
        if (show_fig==1)
            figure 
            %show(roombot,config);
        end 
        orient=[acm_fixed init_orientation(1,1:3) init_orientation(2,1:3) init_orientation(3,1:3) rad2deg(m0)];
        orient=int2str(orient);
        orient(orient == ' ') = [];

        blocked='';
        if (show_fig==1)
            hold on 
            plotVoxels(b,g,d,'b',1);
        end 
        for i=1:4
             
            config(2).JointPosition=pi/4+(i-1)*pi/2;
            tform = getTransform(roombot,config,'body6');
            [acell,cell]=pos_to_cell(tform,EE_pose,cell_ee);
            b=zeros(3,3,3);
            b(cell(1),cell(2),cell(3))=1;
            if (show_fig==1)
               hold on
               %show(roombot,config);
               plotVoxels(b,g,d,'r',alpha);
               pause(1)
            end 
            blocked {i} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
        end 
        if (show_fig==1)
            view(150,20); 
            xlim([-1,3]);
            ylim([-1,3]);
            zlim([-1,3]);
            xlabel('x')
        end 
        fprintf(fileID,'Collision_LookUpTable.Add("%s", new int[] {%s, %s , %s , %s}); \n',orient,blocked{1},blocked{2},blocked{3},blocked {4});
    end 
end 
%pause
end 
    fclose(fileID);
%%

function [acell,cell]=pos_to_cell(tform,ee_pose,cell_ee)
z=find(int8(abs(tform(1:3,3)))==1);
cell=cell_ee;
acell=zeros(size(cell_ee));
switch(z)
    case 1
        side_pose=tform([2 3],4);
        diff=(side_pose-ee_pose([2 3]));
        acell([2 3])=(round(diff));
        cell([2 3])=cell([2 3])+acell([2 3]); 
        
    case 2
        side_pose=tform([1 3],4);
        diff=side_pose-ee_pose([1 3]);
        acell([1 3])=round(diff);
        cell([1 3])=cell([1 3])+ acell([1 3]);
        
    case 3
        side_pose=tform([1 2],4);
        diff=side_pose-ee_pose([1 2]);
        acell([1 2])=round(diff);
        cell([1 2])=cell([1 2])+acell([1 2]);       
end 
end   
function [ACM1_Voxel]=findvoxel(H_ACM1,ACM0_Voxel)
round(H_ACM1);
round(ACM0_Voxel);
div = 2;
for i =1:3
difference = round(2*H_ACM1(i, 4) -2* (ACM0_Voxel(i)-1));
if (i == 3)
    difference = round(2*H_ACM1(i, 4) -2* (ACM0_Voxel(i)-1+1/2));
    div = 2;
end 
ACM1_Voxel(i) = ACM0_Voxel(i) + fl_cl(difference / div) ;%+ fl_cl(mod(difference + 1,2) / 2);
end
end 
function out=fl_cl(in)
if in<0 
    out=ceil(in);
else
    out=floor(in);
end
end 