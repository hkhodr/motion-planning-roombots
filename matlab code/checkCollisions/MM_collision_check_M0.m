%% Check Collision for Metamodules: 
clc;
clear all; 
close all;

figure_on=true;
acm_fixed=0;

%% vexel options
size=6;
g.x=[0,1,2,3,4,5,6];
g.y=[0,1,2,3,4,5,6];
g.z=[1/2,1.5,2.5,3.5,4.5,5.5,6.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.5;      %default transparency is 0.5


ee1=[1,1,0];
ee2=[-1,0,1];
ee3=[0,-1,1];
acm2_fixed=0;
side=1;
if acm_fixed==0
    s=-1;
    %ee2=[1,1];
else
    s=1;
    %ee2=[-1,-1];
end 
different_orientations=[0 0 0 ;
                        0  pi/2 0 ;
                        -pi/2 0 pi/2;
                        0 -pi/2 0;
                        pi/2 0 -pi/2;
                        pi 0 -pi];%[z,x,y,-x,-z,-y]
         
init_point=2*ones(6,3)+...
                    [1 1 1;
                    0.5,1,1.5;
                    1,0.5,1.5;
                    1.5,1,1.5;
                    1,1.5,1.5;
                    1 1 2];
            
yaw=[0,pi/2,pi,3*pi/2];                    
fileID = fopen(sprintf('MM_Voxel_Occupied_M0_%d.txt',acm_fixed), 'w');
contact_bodies={'body6';'body26';'body1';'body21'};

contact_bodiesB1={'body55';'body56';'body57';'body58'};
contact_bodiesB2={'body255';'body256';'body257';'body258'};

cells_visited=[-1 -1 -1];
for or=1:1
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ');
        
for y=1:1
    
    init_orientation=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
    %initialiaztion
    init_orientation(1:3,4)=init_point(or,:);
    b=zeros(size,size,size);
    roombot = init_roombot_metamodule(acm_fixed,acm2_fixed,init_orientation,init_orientation1,side,ee1,ee2,ee3);
    
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
            b=zeros(size,size,size);
            m0=motors(m);
            if(acm_fixed==0)
                m1=-m0;
                mdesired1=-mdesired;
            else
                m1=m0;
                mdesired1=mdesired;
            end 
            %Initial position
            
            config=roombot.homeConfiguration;
            config(1).JointPosition=m1;
            config(2).JointPosition=0;
            config(3).JointPosition=0;
            
            show(roombot,config)
            BaseH = getTransform(roombot,config,'body0');
            BaseVx=BaseH(1:3,4)+1;
            
            B1tform = getTransform(roombot,config,'body1');
            B2tform = getTransform(roombot,config,'body21');
            EEtform1 = getTransform(roombot,config,'body4');
            EEtform2 = getTransform(roombot,config,'body24');
            EE_pose1=(EEtform1(1:3,4));
            EE_pose2=(EEtform2(1:3,4));

            cell_ee=findvoxel(EEtform1,BaseVx);
            cell_ee2=findvoxel(EEtform2,BaseVx);
            cell_b1=findvoxel(B1tform,BaseVx);
            cell_b2=findvoxel(B2tform,BaseVx);
            
            
            b(cell_ee(1),cell_ee(2),cell_ee(3))=1;
            b(cell_ee2(1),cell_ee2(2),cell_ee2(3))=1;
            b(cell_b1(1),cell_b1(2),cell_b1(3))=1;
            b(cell_b2(1),cell_b2(2),cell_b2(3))=1;
            
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
            pause
            end
            
            n=abs(mdesired1-m1)/(pi/6);
            b=zeros(size,size,size);  
            cell_visited=[];
            for i=1:n                
                config(1).JointPosition=m1+(sign(mdesired1-m1))*(pi/6+(i-1)*pi/6);
%                 if i==n
%                     tform = getTransform(roombot,config,'body4');
%                     tform2 = getTransform(roombot,config,'body24');
%                     tform3 = getTransform(roombot,config,'body21');
%                     tform4 = getTransform(roombot,config,'body2');
%                     
%                 else
%                     tform = getTransform(roombot,config,'body6');
%                     tform2 = getTransform(roombot,config,'body26');
%                     tform3 = getTransform(roombot,config,'body31');
%                     tform4 = getTransform(roombot,config,'body3');
%                 end 
                for kk=1:4
                   tform=getTransform(roombot,config,contact_bodiesB2{kk});
                   cell=findvoxel(tform,BaseVx);
                   if ~any(ismember(cells_visited,cell,'rows'))
                       cells_visited=[cells_visited;cell];
                       b(cell(1),cell(2),cell(3))=1;
                       if i==1
                            blocked = sprintf('%d ,%d, %d ',(cell(1)),(cell(2)),(cell(3)));
                       else
                            blocked = strcat(blocked,sprintf(', %d ,%d, %d',(cell(1)),(cell(2)),(cell(3))));
                       end 
                   end 
                c='r';
                end 
%                 if i==n
%                     c='b';
%                     acell_ee=acell;
%                     acell_ee2=acell2;
%                     blocked {i+1} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
%                     blocked {i+2} = sprintf('%d ,%d, %d',(acell2(1)),(acell2(2)),(acell2(3)));
%                 else 
%                     blocked {i} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
%                     blocked {i+1} = sprintf('%d ,%d, %d',(acell2(1)),(acell2(2)),(acell2(3)));
%                 end 
                
%                 b(cell(1),cell(2),cell(3))=1;
%                 b(cell2(1),cell2(2),cell2(3))=1;
%                 b(cell3(1),cell3(2),cell3(3))=1;
%                 b(cell4(1),cell4(2),cell4(3))=1;
                if figure_on
                    show(roombot,config);
                    plotVoxels(b,g,d,c,alpha);
                end 
                pause
                
            end 
%             VX0=BaseVx+1;
%             b=zeros(size,size,size);
%             y0=find(abs(init_orientation(1:3,2))==1);
%             acell=VX0-cell_ee;
%             acell(y0)=acell(y0)+s*init_orientation(y0,2);
%             Vx0=cell_ee+acell;
%             if any(Vx0~=cell_ee) && any(acell~=acell_ee)
%             b(Vx0(1),Vx0(2),Vx0(3))=1;
%             if figure_on
%             plotVoxels(b,g,d,'r',alpha)
%             end
%             blocked {i} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
%             end 
%             
%             x0=find(abs(init_orientation(1:3,1))==1);
%             b=zeros(size,size,size);
%             acell=VX0-cell_ee;
%             acell(x0)=acell(x0)+s*init_orientation(x0,1);
%             Vx0=cell_ee+acell;
%             if any(Vx0~=cell_ee) && any(acell~=acell_ee)
%             b(Vx0(1),Vx0(2),Vx0(3))=1;
%             if figure_on
%             plotVoxels(b,g,d,'r',alpha)
%             end 
%             blocked {i} = sprintf('%d ,%d, %d',(acell(1)),(acell(2)),(acell(3)));
%             end 
            if figure_on
                show(roombot,config);
                view(150,20); 
                xlim([-1,7]);
                ylim([-1,7]);
                zlim([0,7]);
                xlabel('x')
            end 
            fprintf(fileID,'Collision_LookUpTable.Add("%s", new int[] {%s}); \n',orient,blocked);
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