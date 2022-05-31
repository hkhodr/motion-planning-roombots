function [roombot] = init_roombot_module(pos_0_1,tform0,ee2)

if pos_0_1==0
    %DH param
    d=[0,0.5,0,1,0,-0.5];
    theta=[0,-pi/4,0,0,0,pi/4+pi];
    a=deg2rad(55);
    alpha=[0,a -a a -a-pi 0];
    r=[0,0,0,0,0,0];
    dhparam=[r' alpha' d' theta'];
else 
    %DH param
    d=[0,0.5,0,1,0,-0.5];
    theta=[0,-pi/4,0,0,0,pi/4+pi];
    a=deg2rad(125);
    alpha=[0,a -a a -a-pi 0];
    r=[0,0,0,0,0,0];
    dhparam=[r' alpha' d' theta'];
end 
%%
body0 = robotics.RigidBody('body0');
jnt0 = robotics.Joint('jnt0','fixed');  
setFixedTransform(jnt0,tform0);
body0.Joint = jnt0;

body1 = robotics.RigidBody('body1');
jnt1 = robotics.Joint('jnt1','fixed');
setFixedTransform(jnt1,dhparam(2,:),'dh');
body1.Joint = jnt1;

body2 = robotics.RigidBody('body2');
jnt2 = robotics.Joint('jnt2','revolute');
setFixedTransform(jnt2,dhparam(3,:),'dh');
body2.Joint = jnt2;

body3 = robotics.RigidBody('body3');
jnt3 = robotics.Joint('jnt3','revolute');
setFixedTransform(jnt3,dhparam(4,:),'dh');
body3.Joint = jnt3;

body4 = robotics.RigidBody('body4');
jnt4 = robotics.Joint('jnt4','revolute');
setFixedTransform(jnt4,dhparam(5,:),'dh');
body4.Joint = jnt4;

body5 = robotics.RigidBody('EndEffector');
jnt5 = robotics.Joint('jnt5','fixed');
setFixedTransform(jnt5,dhparam(6,:),'dh');
body5.Joint = jnt5;

body6 = robotics.RigidBody('body6');
jnt6 = robotics.Joint('jnt6','fixed');
setFixedTransform(jnt6,[1 0 0 ee2(1)/2; 0 1 0 ee2(2)/2; 0 0 1 0; 0 0 0 1]);
body6.Joint = jnt6;

%body5 = robotics.RigidBody('EndEffector');
% jnt5 = robotics.Joint('jnt5','fixed');
% setFixedTransform(jnt5,dhparam(6,:),'dh');
% body5.Joint = jnt5;
% 
% body5 = robotics.RigidBody('EndEffector');
% jnt5 = robotics.Joint('jnt5','fixed');
% setFixedTransform(jnt5,dhparam(6,:),'dh');
% body5.Joint = jnt5;

%%
roombot = robotics.RigidBodyTree;
addBody(roombot,body0,'base');
addBody(roombot,body1,'body0');
addBody(roombot,body2,'body1'); % Add body2 to body1
addBody(roombot,body3,'body2'); %
addBody(roombot,body4,'body3'); %
addBody(roombot,body5,'body4'); %
addBody(roombot,body6,'EndEffector'); %

end

