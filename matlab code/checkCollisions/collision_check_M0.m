% Check Collision: 
clc;
clear all; 
close all;

figure_on=true;
acm_fixed=1;

%vexel options 
g.x=[0,1,2,3,4,5];
g.y=[0,1,2,3,4,5];
g.z=[1/2,1.5,2.5,3.5,4.5,5.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2



if acm_fixed==0
    s=-1;
    ee2=[1,1];
else
    s=1;
    ee2=[-1,-1];
end 
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
fileID = fopen(sprintf('Voxel_Occupied_M0_%d.txt',acm_fixed), 'w');

for or=1:6
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ');
        
for y=1:4
    
    init_orientation=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
    %initialiaztion
    b=zeros(5,5,5);
    roombot = init_roombot_module(acm_fixed,init_orientation,ee2);
    motors=[0,2*pi/3,-2*pi/3];
   
    for m=1:length(motors)
        switch m
            case 1
                md=[2*pi/3,-2*pi/3];
            case 2
                md=0;
            case 3
                md=0;
        end
        for ii=1:length(md)
            mdesired=md(ii);
            b=zeros(5,5,5);
            m0=motors(m);
            cell_matrix=[0 0 0];
            cell_matrix_count=0;
            if(acm_fixed==0)
                m1=-m0;
                mdesired1=-mdesired;
            else
                m1=m0;
                mdesired1=mdesired;
            end 
            %Initial position.
            config=roombot.homeConfiguration;
            config(1).JointPosition=m1;
            config(2).JointPosition=0;
            config(3).JointPosition=0;
            tform = getTransform(roombot,config,'body6');
            EEtform = getTransform(roombot,config,'body4');
            EE_pose=(EEtform(1:3,4));

            cell_ee=findvoxel(EEtform,[3,3,3]);
            b(cell_ee(1),cell_ee(2),cell_ee(3))=1;
            if figure_on
            figure
            show(roombot,config);
            end 
            orient=[acm_fixed init_orientation(1,1:3) init_orientation(2,1:3) init_orientation(3,1:3) rad2deg(m0) rad2deg(mdesired)];
            orient=int2str(orient);
            orient(orient == ' ') = [];

            blocked='';
            if figure_on
            hold on 
            plotVoxels(b,g,d,c,alpha);
            end
            b=zeros(5,5,5);
            blocked={};
            n=abs(mdesired1-m1)/(pi/6);    
            for i=1:n
                
                config(1).JointPosition=m1+(sign(mdesired1-m1))*(pi/6+(i-1)*pi/6);
                if i==n
                    tform = getTransform(roombot,config,'body4');
                else
                    tform = getTransform(roombot,config,'body6');
                    tform2=getTransform(roombot,config,'body3');
                    
                    [acell2,cell2]=pos_to_cell2(tform2,EE_pose,cell_ee);
                    if ~any(ismember(cell_matrix,acell2,'rows'))
                        cell_matrix=[cell_matrix;acell2];
                        cell_matrix_count=cell_matrix_count+1;
                        blocked {cell_matrix_count} = sprintf('%d ,%d, %d',(acell2(1)),(acell2(2)),(acell2(3)));
                        b(cell2(1),cell2(2),cell2(3))=1;
                    end 
                end 
                [acell,cell]=pos_to_cell(tform,EE_pose,cell_ee);
                if ~any(ismember(cell_matrix,acell,'rows'))
                    cell_matrix=[cell_matrix;acell];
                    cell_matrix_count=cell_matrix_count+1;
                    blocked {cell_matrix_count} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
                end
                if i==n
                    acell_ee=acell;
                    idx = strcmp(blocked,sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3))));
                    blocked(idx) = [];
                    blocked {end+2} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
                end 
                cell
                b(cell(1),cell(2),cell(3))=1;
                if figure_on
                show(roombot,config);
                plotVoxels(b,g,d,'r',alpha);
                end 
                %pause
                
            end 
            VX0=[3,3,3];
            b=zeros(5,5,5);
            y0=find(abs(init_orientation(1:3,2))==1);
            acell=VX0-cell_ee;
            acell(y0)=acell(y0)+s*init_orientation(y0,2);
            Vx0=cell_ee+acell;
            if any(Vx0~=cell_ee) && any(acell~=acell_ee)
            b(Vx0(1),Vx0(2),Vx0(3))=1;
            if figure_on
            plotVoxels(b,g,d,'r',alpha)
            end
            blocked {end-1} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
            end 
            
            x0=find(abs(init_orientation(1:3,1))==1);
            b=zeros(5,5,5);
            acell=VX0-cell_ee;
            acell(x0)=acell(x0)+s*init_orientation(x0,1);
            Vx0=cell_ee+acell;
            if any(Vx0~=cell_ee) && any(acell~=acell_ee)
            b(Vx0(1),Vx0(2),Vx0(3))=1;
            if figure_on
            plotVoxels(b,g,d,'r',alpha)
            end 
            blocked {end-1} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
            end 
            if figure_on
            view(150,20); 
            xlim([-1,4]);
            ylim([-1,4]);
            zlim([0,5]);
            xlabel('x')
            end 
            fprintf(fileID,'Collision_LookUpTable.Add("%s", new int[] {%s, %s , %s,%s,%s,%s}); \n',orient,blocked{1},blocked{2},blocked{3},blocked{4},blocked{5},blocked{6});
        end 
    end
end
end

%pause
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

function [acell,cell]=pos_to_cell2(tform,ee_pose,cell_ee)
    cell=cell_ee;
    acell=zeros(size(cell_ee));
    side_pose=tform(1:3,4);
    diff=side_pose-ee_pose;
    for i=1:3
    if(diff(i)>0.7)
        acell(i)=ceil(diff(i));
    else
        acell(i)=floor(diff(i));
    end 
    end 
    acell=round(diff)';
    %acell=round(diff)';
    cell=cell+acell; 
end  
function [ACM1_Voxel]=findvoxel(H_ACM1,ACM0_Voxel)
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