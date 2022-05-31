%Notes: tested with acm1 (a=-55 deg), different config: GOOD 

%close all 
clear all 
%clc 

% Setting fonts and lines 
set(groot, 'DefaultAxesFontSize',14);
set(groot, 'DefaultLineLineWidth',3);

%initial conditions 
thetai0=0;
thetai1=0;
thetai2=0;
pitch=0;%y axis 
roll=0; %x axis
yaw=0;  %z axis 

%DH param
d=[0,0.5,0,1,0,-0.5]*2;
theta=[0,-pi/4,0,0,0,pi/4+pi];
a=deg2rad(125);
alpha=[0,a -a a -a-pi 0];
r=[0,0,0,0,0,0];
dhparam=[r' alpha' d' theta'];

%%
body0 = robotics.RigidBody('body0');
jnt0 = robotics.Joint('jnt0','fixed');
%set the initial orientation based on the Homogeneous matrix 
tform0 =[0     -1     0     0;
     1     0     0     2;
    0     0     1     0;
     0     0     0     1];
%tform0=eye(4);
setFixedTransform(jnt0,tform0);%eul2tform([roll pitch yaw],'XYZ'));%dhparam(1,:),'dh');
% OR based on roll pitch yaw 
%roll=pi/2;
%yaw=pi/2;
%setFixedTransform(jnt0,eul2tform([roll pitch yaw],'XYZ'));%dhparam(1,:),'dh');
body0.Joint = jnt0;

body1 = robotics.RigidBody('body1');
jnt1 = robotics.Joint('jnt1','fixed');
setFixedTransform(jnt1,dhparam(2,:),'dh');
body1.Joint = jnt1;

body2 = robotics.RigidBody('body2');
jnt2 = robotics.Joint('jnt2','revolute');
setFixedTransform(jnt2,dhparam(3,:),'dh');
jnt2.HomePosition=thetai0;
body2.Joint = jnt2;

body3 = robotics.RigidBody('body3');
jnt3 = robotics.Joint('jnt3','revolute');
setFixedTransform(jnt3,dhparam(4,:),'dh');
body3.Joint = jnt3;
jnt3.HomePosition=thetai1;

body4 = robotics.RigidBody('body4');
jnt4 = robotics.Joint('jnt4','revolute');
setFixedTransform(jnt4,dhparam(5,:),'dh');
body4.Joint = jnt4;
jnt4.HomePosition=thetai2;

body5 = robotics.RigidBody('EndEffector');
jnt5 = robotics.Joint('jnt5','fixed');
setFixedTransform(jnt5,dhparam(6,:),'dh');
body5.Joint = jnt5;

%%
roombot = robotics.RigidBodyTree;
addBody(roombot,body0,'base');
addBody(roombot,body1,'body0');
addBody(roombot,body2,'body1'); % Add body2 to body1
addBody(roombot,body3,'body2'); %
addBody(roombot,body4,'body3'); %
addBody(roombot,body5,'body4'); %
config=roombot.homeConfiguration;
%%
config(1).JointPosition=-deg2rad(120);%
config(2).JointPosition=-deg2rad(90); %this is minus theta 
config(3).JointPosition=-deg2rad(120);
tform = getTransform(roombot,config,'EndEffector')
eul = rad2deg(rotm2eul(tform(1:3,1:3),'XYZ'))
show(roombot,config);
view(170,41);
%%
% Test the different angle combination: 
t0=[0,2*pi/3,-2*pi/3];
t1=[0,pi/2,pi,-pi/2];
t2=[0,2*pi/3,-2*pi/3];
config(1).JointPosition=0;
config(2).JointPosition=0;
config(3).JointPosition=0;
figure
for i=1:3
subplot(1,3,i)
config(3).JointPosition=t2(i);
tform = getTransform(roombot,config,'EndEffector','base');
eul = rad2deg(rotm2eul(tform(1:3,1:3),'XYZ'));
show(roombot,config);
view([2,1,2])
end 
figure
config(1).JointPosition=0;
config(2).JointPosition=0;
for i=1:4
subplot(1,4,i)
config(2).JointPosition=t1(i);
tform = getTransform(roombot,config,'EndEffector','base');
eul = rad2deg(rotm2eul(tform(1:3,1:3),'XYZ'));
show(roombot,config);
view([2,1,2])
end 
figure
config(1).JointPosition=0;
config(2).JointPosition=0;
config(3).JointPosition=0;
for i=1:3
subplot(1,3,i)
config(1).JointPosition=t0(i);
tform = getTransform(roombot,config,'EndEffector','base');
eul = rad2deg(rotm2eul(tform(1:3,1:3),'XYZ'));
show(roombot,config);
view([2,1,2])
end 