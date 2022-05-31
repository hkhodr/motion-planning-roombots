% Check Collision: 
clc;
clear all; 
close all;
ee2=[1,-1];
%vexel options 
g.x=[0,1,2,3,4,5];
g.y=[0,1,2,3,4,5];
g.z=[1/2,1.5,2.5,3.5,4.5,5.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2

acm_fixed=1;
figure_on=true;
different_orientations=[0 0 0 ;
                        0  pi/2 0 ;
                        -pi/2 0 pi/2;
                        0 -pi/2 0;
                        pi/2 0 -pi/2;
                        pi 0 -pi];%[z,x,y,-x,-z,-y]
         
init_point=ones(6,3)+...
                    [1 1 1;
                    0.5,1,1.5;
                    1,0.5,1.5;
                    1.5,1,1.5;
                    1,1.5,1.5;
                    1 1 2];
            
yaw=[0,pi/2,pi,3*pi/2];                    
fileID = fopen(sprintf('Voxel_Occupied_M2_%d.txt',acm_fixed), 'w');

for or=1:1
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ');

for y=1:1
    
    init_orientation=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
    %initialiaztion
    init_orientation(1:3,4)=init_point(or,:);
    b=zeros(5,5,5);
    roombot = init_roombot_module(acm_fixed,init_orientation,ee2);
    motors=[0,2*pi/3,-2*pi/3];
    motors1=[0,-pi/2,pi/2,pi];
    for m=1:length(motors)
        for mm=1:length(motors1)
        b=zeros(5,5,5);
        m0=motors(m);
        
        if(acm_fixed==0)
            m1=-m0;
        else
            m1=m0;
        end 
        %Initial position.
        config=roombot.homeConfiguration;
        config(1).JointPosition=m1;
        config(2).JointPosition=-motors1(mm);
        config(3).JointPosition=0;
        tform = getTransform(roombot,config,'body6');
        EEtform = getTransform(roombot,config,'body4');
        EE_pose=(EEtform(1:3,4));

        cell_ee=findvoxel(EEtform,[3,3,3]);
        b(cell_ee(1),cell_ee(2),cell_ee(3))=1;
        b(cell_ee(1),cell_ee(2),cell_ee(3)-1)=1;
        if figure_on
        figure 
        
        %show(roombot,config);
        end 
        orient=[acm_fixed init_orientation(1,1:3) init_orientation(2,1:3) init_orientation(3,1:3) rad2deg(m0) rad2deg(motors1(mm))];
        orient=int2str(orient);
        orient(orient == ' ') = [];

        blocked='';
        if figure_on
            hold on 
            plotVoxels(b,g,d,'b',1);
        end 
        for i=1:3
            
            config(3).JointPosition=pi/3+(i-2)*2*pi/3;
            tform = getTransform(roombot,config,'body6');
            [acell,cell]=pos_to_cell(tform,EE_pose,cell_ee);
            b=zeros(5,5,5);
            b(cell(1),cell(2),cell(3))=1;
            if figure_on
            %show(roombot,config);
            plotVoxels(b,g,d,'r',alpha);
            pause(1)
            end
            blocked {i} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
        end 
        if figure_on
        view(150,20); 
        xlim([-1,4]);
        ylim([-1,4]);
        zlim([0,5]);
        xlabel('x')
        end 
       
        fprintf(fileID,'Collision_LookUpTable.Add("%s", new int[] {%s, %s , %s}); \n',orient,blocked{1},blocked{2},blocked{3});
        end
    end
end 
%pause
end 
    fclose(fileID);
%%

function [acell,cell]=pos_to_cell(tform,ee_pose,cell_ee)
%ee_pose=[1,1,3]';
    cell=cell_ee;
    acell=zeros(size(cell_ee));
    side_pose=tform(1:3,4);

    diff=side_pose-ee_pose;
    acell=round(diff)';
    cell=cell+acell; 
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