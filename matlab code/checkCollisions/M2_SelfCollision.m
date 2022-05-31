%M2 self collision: 

clear all 
clc 
close all 
acm=1;
figure_on=false;
%%
RBPOS{1}={[0 0 1; 0 -1 1;1 -1 1],[0 0 0 ; -1 0 1; -1 -1 1],[0 0 1; 0 1 1; -1 1 1],[0 0 1;1 0 1; 1 1 1],...
        [0 0 1; -1 0 1; -1 1 1],[0 0 1;0 1 1; 1 1 1],[0 0 1; 1 0 1;1 -1 1],[0 0 1; 0 -1 1;-1 -1 1],...
        [0 0 1;-1 0 1;-1 0 0],[0 0 1;0 1 1; 0 1 0],[0 0 1;1 0 1;1 0 0],[0 0 1;0 -1 1;0 -1 0],...
        [0 0 1;0 -1 1;0 -1 0],[0 0 1;-1 0 1;-1 0 0],[0 0 1;0 1 1;0 1 0],[0 0 1;1 0 1;1 0 0]};
    
RBPOS{2}={[0 0 1; 0 -1 1;1 -1 1],[0 0 0 ; -1 0 1; -1 -1 1],[0 0 1; 0 1 1; -1 1 1],[0 0 1;1 0 1; 1 1 1],...
        [0 0 1; -1 0 1; -1 1 1],[0 0 1;0 1 1; 1 1 1],[0 0 1; 1 0 1;1 -1 1],[0 0 1; 0 -1 1;-1 -1 1],...
        [0 0 1;-1 0 1;-1 0 0],[0 0 1;0 1 1; 0 1 0],[0 0 1;1 0 1;1 0 0],[0 0 1;0 -1 1;0 -1 0],...
        [0 0 1;0 -1 1;0 -1 0],[0 0 1;-1 0 1;-1 0 0],[0 0 1;0 1 1;0 1 0],[0 0 1;1 0 1;1 0 0]};
if acm==0
    RB_m1{1}={0,90,180,-90,0,90,180,-90,0,90,180,-90,0,90,180,-90};
    RB_m1{2}={-90,0,90,180,-90,0,90,180,-90,0,90,180,-90,0,90,180};
    RB_m2{1}={120,120,120,120,-120,-120,-120,-120,-120,-120,-120,-120,120,120,120,120};
    RB_m2{2}={-120,-120,-120,-120,120,120,120,120,120,120,120,120,-120,-120,-120,-120};
    init=eye(4);
else
    RB_m1{1}={180,-90,0,90,180,-90,0,90,180,-90,0,90,180,-90,0,90};
    RB_m1{2}={-90,0,90,180,-90,0,90,180,-90,0,90,180,-90,0,90,180};
    RB_m2{1}={-120,-120,-120,-120,120,120,120,120,120,120,120,120,-120,-120,-120,-120};
    RB_m2{2}={120,120,120,120,-120,-120,-120,-120,-120,-120,-120,-120,120,120,120,120};
     init=eul2tform([0 0 pi/2],'XYZ');
     %init=eye(4);
end 

RB_m0={0 2*pi/3 -2*pi/3};
%%
figure_on=false;
b=zeros(7,7,7);
b(4,4,4)=1;
%vexel options 
g.x=[0,1,2,3,4,5,6];
g.y=[0,1,2,3,4,5,6];
g.z=[1/2,1.5,2.5,3.5,4.5,5.5,6.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2

%%

dhparam0=dh_(1);
body0 = robotics.RigidBody('body0');
jnt0 = robotics.Joint('jnt0','fixed');  
setFixedTransform(jnt0,eye(4));
body0.Joint = jnt0;

body1 = robotics.RigidBody('body1');
jnt1 = robotics.Joint('jnt1','fixed');
setFixedTransform(jnt1,dhparam0(2,:),'dh');
body1.Joint = jnt1;

body2 = robotics.RigidBody('body2');
jnt2 = robotics.Joint('jnt2','revolute');
setFixedTransform(jnt2,dhparam0(3,:),'dh');
body2.Joint = jnt2;

body3 = robotics.RigidBody('body3');
jnt3 = robotics.Joint('jnt3','revolute');
setFixedTransform(jnt3,dhparam0(4,:),'dh');
body3.Joint = jnt3;

body4 = robotics.RigidBody('body4');
jnt4 = robotics.Joint('jnt4','revolute');
setFixedTransform(jnt4,dhparam0(5,:),'dh');
body4.Joint = jnt4;

body5 = robotics.RigidBody('body5');
jnt5 = robotics.Joint('jnt5','fixed');
tf=eye(4);
tf(:,4)=[0 0 -1.5 1];
setFixedTransform(jnt5,tf);
body5.Joint = jnt5;

roombot = robotics.RigidBodyTree;
addBody(roombot,body0,'base');
addBody(roombot,body1,'body0');
addBody(roombot,body2,'body1');
addBody(roombot,body3,'body2');
addBody(roombot,body4,'body3');
addBody(roombot,body5,'body4');

config=roombot.homeConfiguration;
config(2).JointPosition=pi/4;
%show(roombot,config);
%figure

%%  Write to file 
different_orientations=[0 0 0 ;
                        0  pi/2 0 ;
                        -pi/2 0 pi/2;
                        0 -pi/2 0;
                        pi/2 0 -pi/2;
                        pi 0 -pi];%[z,x,y,-x,-z,-y]

yaw=[0,pi/2,pi,3*pi/2];                    
fileID = fopen(sprintf('MM_Voxel_Occupied_SM2_%d.txt',acm), 'w');
s='';
count=0;
keys={};
for or=1:6
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ'); 
    init_orientation2=init*eul2tform(different_orientations(or,:),'XYZ');        
    for y=1:4 
        b=zeros(7,7,7);
        b(4,4,4)=1;
        init_orientation=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ'); 
        init_orientationf=init_orientation2*eul2tform([0 0 yaw(y)],'XYZ'); 
        orientation=[acm init_orientationf(1,1:3) init_orientationf(2,1:3) init_orientationf(3,1:3)];
        for ii=1:2
        for m0=1:3
            config(1).JointPosition=RB_m0{m0};
            change_m0 = round(getTransform(roombot,config,'body5'));
            change_m0(:,4)=[0 0 0 1];
        for k=1:length(RBPOS{ii})
            orient=orientation;
            orient=[orient RB_m1{ii}{k} RB_m2{ii}{k}]
            for i=1:3
                new=round(init_orientation*change_m0*[RBPOS{ii}{k}(i,:) 1]');
                orient=[orient new(1) new(2) new(3)]; 
                b(new(1)+4, new(2)+4, new(3)+4)=1;
            end 
            if figure_on
            plotVoxels(b,g,d,'r',alpha)
            view(150,20); 
            xlim([-1,7]);
            ylim([-1,7]);
            zlim([0,7]);
            pause
            clf
            hold on 
            end 
            orient=int2str(orient);
            orient(orient == ' ') = [];

            if ~any(strcmp(keys,orient))
                keys{end+1}=orient;
                s=sprintf('1'); 
                fprintf(fileID,'CollisionMM_LookUpTable.Add("%s", new int[] {%s}); \n',orient,s);
            else
                ii
                k
                count=count+1
                if k<10
                    ii
                end 

            end
        end 
        end 
        end
    end
end 
fclose(fileID);

function [dhparam]=dh_(pos)
if pos==0
    %DH param
    d=[0,0.5,0,1,0,-0.5];
    theta=[0,-pi/4,0,0,0,pi/4+pi];
    a=deg2rad(55);
    alpha=[0,a -a a -a -pi ];%0];
    r=[0,0,0,0,0,0];
    dhparam=[r' alpha' d' theta'];
else 
    %DH param
    d=[0,0.5,0,1,0,-0.5];
    theta=[0,-pi/4,0,0,0,pi/4+pi];
    a=deg2rad(125);
    alpha=[0,a -a a -a -pi];
    r=[0,0,0,0,0,0];
    dhparam=[r' alpha' d' theta'];
end 
end 
