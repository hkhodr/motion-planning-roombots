%function [pitch,theta0,theta1,theta2]= inverse_kinematics(x,y,z,wx,wy,wz,thetai0,thetai1,thetai2,axi,ayi,azi)

thetai0=0;thetai1=0;thetai2=0;
wx=-pi;wy=0;wz=-pi/2;
x=0;y=0;z=2;
xc=2063;yc=2026;zc=2035;
pitch=deg2rad(-1);% atan2(-(azi-zc),sqrt((axi-xc)^2+(ayi-yc)^2));
roll=0;
yaw=0;%-deg2rad(3);
%param
d=[0,0.5,0,1,0,-0.5];
theta=[0,-pi/4,0,0,0,pi/4+pi];
a=deg2rad(55);
alpha=[0,a -a a -a-pi 0];
r=[0,0,0,0,0,0];
dhparam=[r' alpha' d' theta'];

body0 = robotics.RigidBody('body0');
jnt0 = robotics.Joint('jnt0','fixed');
setFixedTransform(jnt0,eul2tform([roll pitch yaw],'XYZ'));%dhparam(1,:),'dh');
body0.Joint = jnt0;
roombot = robotics.RigidBodyTree;
addBody(roombot,body0,'base');

body1 = robotics.RigidBody('body1');
jnt1 = robotics.Joint('jnt1','fixed');
setFixedTransform(jnt1,dhparam(2,:),'dh');
body1.Joint = jnt1;
addBody(roombot,body1,'body0');

body2 = robotics.RigidBody('body2');
jnt2 = robotics.Joint('jnt2','revolute');
setFixedTransform(jnt2,dhparam(3,:),'dh');
jnt2.HomePosition=thetai0;
body2.Joint = jnt2;
addBody(roombot,body2,'body1'); % Add body2 to body1

body3 = robotics.RigidBody('body3');
jnt3 = robotics.Joint('jnt3','revolute');
setFixedTransform(jnt3,dhparam(4,:),'dh');
body3.Joint = jnt3;
jnt3.HomePosition=thetai1;
addBody(roombot,body3,'body2'); % Add body3 to body2

body4 = robotics.RigidBody('body4');
jnt4 = robotics.Joint('jnt4','revolute');
setFixedTransform(jnt4,dhparam(5,:),'dh');
body4.Joint = jnt4;
jnt4.HomePosition=thetai2;
addBody(roombot,body4,'body3'); % Add body3 to body2

body5 = robotics.RigidBody('EndEffector');
jnt5 = robotics.Joint('jnt5','fixed');
setFixedTransform(jnt5,dhparam(6,:),'dh');
body5.Joint = jnt5;
addBody(roombot,body5,'body4'); % Add body3 to body2
show(roombot,roombot.homeConfiguration);
tform = getTransform(roombot,roombot.homeConfiguration,'EndEffector')
%%
tformdesired = [[eul2rotm([wx,wy,wz],'XYZ'),[x y z]'];[0 0 0 1]];
ik = robotics.InverseKinematics('RigidBodyTree',roombot);
weights = [1 1 1 1 1 1];
initialguess = roombot.homeConfiguration;
[configSoln,~] = ik('EndEffector',tformdesired,weights,initialguess);
tform = getTransform(roombot,configSoln,'EndEffector')
figure
show(roombot,configSoln);
theta0=-rad2deg(configSoln(1).JointPosition)
theta1=-rad2deg(configSoln(2).JointPosition)
theta2=-rad2deg(configSoln(3).JointPosition)
pitch=rad2deg(pitch)

%end 