clear all 
clc 
close all 

%  d: offset along previous z to the common normal
% theta: angle about previous z, from old x to new x
% r: length of the common normal (aka a, but if using this notation, do not confuse with \alpha ). Assuming a revolute joint, this is the radius about previous z.
% alpha: angle about common normal, from old z axis to new z axis

syms t0 t1 t2 a b;

% d=[0,1/2,0,1,0,1/2];
% theta=[0,-b,t0,t1,t2,b];
% alpha=[0,a -a a -a 0];
% r=[0,0,0,0,0,0];

d=[0,1,0,2,0,-1];
theta=[0,-b,t0,-t1,t2,+b+pi];
%a=-deg2rad(125);
alpha=[0,a -a a -a-pi 0];
r=[0,0,0,0,0,0];

for i=1:6
    T{i}=A(d(i),theta(i),alpha(i),r(i));
end 
HH=T{1}*T{2}*T{3}*T{4}*T{5}*T{6};

%%
theta0= -2*pi/3;%2*pi/3;
theta1=  pi/2;
theta2= -2*pi/3;
new_pos=coord(1,cos(theta0),sin(theta0),cos(theta1),sin(theta1),cos(theta2),sin(theta2))
H=orientation(1,cos(theta0),sin(theta0),cos(theta1),sin(theta1),cos(theta2),sin(theta2))
%tform0 =[-1 0 0 0; 0 0 1 1;0 1 0 3; 0 0 0 1];   

tform0 =[0     -1     0     0;
     1     0     0     2;
    0     0     1     0;
     0     0     0     1];
before=[[H,new_pos'];[0 0 0 1]]
finale=tform0*[[H,new_pos'];[0 0 0 1]]

result=multiply(tform0,before)

eul = rad2deg(rotm2eul(H(1:3,1:3),'XYZ'))

function [out]=coord(one,ct0,st0,ct1,st1,ct2,st2)
if(one==0)
    ca= 0.5736;%cos(deg2rad(55));
    sa= 0.8192;%sin(deg2rad(55));
    b=0.7071;%sqrt(2)/2
else 
    ca=-0.5736;%cos(deg2rad(125));
    sa=0.8192;%sin(deg2rad(125));
    b=0.7071;%sqrt(2)/2
end 
out(1)= 2*ca*sa*b*ct0 - sa*ct2*(sa^2*(b*st0 + ca*b - ca*b*ct0) + ca*st1*(b*ct0 + ca*b*st0) - ca*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0)) - 2*ca*sa*b - 2*b*sa*st0 - sa*st2*(st1*(sa^2*b + ca^2*b*ct0 - ca*b*st0) + ct1*(b*ct0 + ca*b*st0)) - ca*(ca*sa*(b*st0 + ca*b - ca*b*ct0) - sa*st1*(b*ct0 + ca*b*st0) + sa*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0));
out(2)= sa*ct2*(sa^2*(b*st0 - ca*b + ca*b*ct0) + ca*st1*(b*ct0 - ca*b*st0) + ca*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) - ca*(sa*st1*(b*ct0 - ca*b*st0) - ca*sa*(b*st0 - ca*b + ca*b*ct0) + sa*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) - 2*ca*b*sa + sa*st2*(ct1*(b*ct0 - ca*b*st0) - st1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) + 2*sa*b*st0 + 2*ca*b*sa*ct0;
out(3)= 2*ca^2 + 2*sa^2*ct0 - ca*(ca*ct0*(ca^2 - 1) - sa^2*st0*st1 - ca^3 + ca*ct1*(ca^2 - 1) + ca*sa^2*ct0*ct1) + sa^2*ct2*(ca^2 - ca^2*ct1 + sa^2*ct0 + ca^2*ct0*ct1 - ca*st0*st1) - sa^2*st2*(ct1*st0 - ca*st1 + ca*ct0*st1) + 1;

end 

function [H]=orientation(one,ct0,st0,ct1,st1,ct2,st2)
if(one==0)
    ca= 0.5736;%cos(deg2rad(-125));
    sa= 0.8192;%sin(deg2rad(-125));
     b= 0.7071;%sqrt(2)/2
else 
    ca=-0.5736;%cos(deg2rad(-125));
    sa= 0.8192;%sin(deg2rad(-125));
     b= 0.7071;%sqrt(2)/2
end 
commun11=b*(ct2*(st1*(sa^2*b + ca^2*b*ct0 - ca*b*st0) + ct1*(b*ct0 + ca*b*st0)) - st2*(sa^2*(b*st0 + ca*b - ca*b*ct0) + ca*st1*(b*ct0 + ca*b*st0) - ca*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0)));
commun12=b*(ca*ct2*(sa^2*(b*st0 + ca*b - ca*b*ct0) + ca*st1*(b*ct0 + ca*b*st0) - ca*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0)) - sa*(ca*sa*(b*st0 + ca*b - ca*b*ct0) - sa*st1*(b*ct0 + ca*b*st0) + sa*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0)) + ca*st2*(st1*(sa^2*b + ca^2*b*ct0 - ca*b*st0) + ct1*(b*ct0 + ca*b*st0)));
H(1,1)= - commun11- commun12;
H(1,2)=   commun11- commun12;

H(1,3)= ca*(ca*sa*(b*st0 + ca*b - ca*b*ct0) - sa*st1*(b*ct0 + ca*b*st0) + sa*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0)) + sa*ct2*(sa^2*(b*st0 + ca*b - ca*b*ct0) + ca*st1*(b*ct0 + ca*b*st0) - ca*ct1*(sa^2*b + ca^2*b*ct0 - ca*b*st0)) + sa*st2*(st1*(sa^2*b + ca^2*b*ct0 - ca*b*st0) + ct1*(b*ct0 + ca*b*st0));
  
commun21=b*(sa*(sa*st1*(b*ct0 - ca*b*st0) - ca*sa*(b*st0 - ca*b + ca*b*ct0) + sa*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) + ca*ct2*(sa^2*(b*st0 - ca*b + ca*b*ct0) + ca*st1*(b*ct0 - ca*b*st0) + ca*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) + ca*st2*(ct1*(b*ct0 - ca*b*st0) - st1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)));
commun22=b*(ct2*(ct1*(b*ct0 - ca*b*st0) - st1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) - st2*(sa^2*(b*st0 - ca*b + ca*b*ct0) + ca*st1*(b*ct0 - ca*b*st0) + ca*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)));

H(2,1)= commun21+commun22;  
H(2,2)= commun21-commun22;

H(2,3)=   ca*(sa*st1*(b*ct0 - ca*b*st0) - ca*sa*(b*st0 - ca*b + ca*b*ct0) + sa*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) - sa*ct2*(sa^2*(b*st0 - ca*b + ca*b*ct0) + ca*st1*(b*ct0 - ca*b*st0) + ca*ct1*(b*ct0*ca^2 + b*st0*ca + b*sa^2)) - sa*st2*(ct1*(b*ct0 - ca*b*st0) - st1*(b*ct0*ca^2 + b*st0*ca + b*sa^2));
commun31=b*(sa*(ca*ct0*(ca^2 - 1) - sa^2*st0*st1 - ca^3 + ca*ct1*(ca^2 - 1) + ca*sa^2*ct0*ct1) + ca*sa*ct2*(ca^2 - ca^2*ct1 + sa^2*ct0 + ca^2*ct0*ct1 - ca*st0*st1) - ca*sa*st2*(ct1*st0 - ca*st1 + ca*ct0*st1));
commun32=b*(sa*ct2*(ct1*st0 - ca*st1 + ca*ct0*st1) + sa*st2*(ca^2 - ca^2*ct1 + sa^2*ct0 + ca^2*ct0*ct1 - ca*st0*st1));

H(3,1)= commun31-commun32; 

H(3,2)= commun31+commun32;

H(3,3)= ca*(ca*ct0*(ca^2 - 1) - sa^2*st0*st1 - ca^3 + ca*ct1*(ca^2 - 1) + ca*sa^2*ct0*ct1) - sa^2*ct2*(ca^2 - ca^2*ct1 + sa^2*ct0 + ca^2*ct0*ct1 - ca*st0*st1) + sa^2*st2*(ct1*st0 - ca*st1 + ca*ct0*st1);
 
end 

function [out]=A(d,O,a,r)
out=[cos(O) -sin(O)*cos(a) sin(O)*sin(a) r*cos(O);
     sin(O) cos(O)*cos(a) -cos(O)*sin(a) r*sin(O);
     0      sin(a)      cos(a)          d;
     0      0           0               1];
end 

function result = multiply(H,A)
result=zeros(4,4);
            for i=1:4
                for j=1:4
                    for k=1:4
                        result(i, j) = result(i, j) + H(i, k) * A(k, j);
                    end 
                end 
            end 
end 