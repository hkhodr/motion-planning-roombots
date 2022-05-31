%xml to list of commands
% Press space to lunch the matlab simulation
close all;
clear all; 
clc ;

%%
for name_id=0:0%39
% name=sprintf("%d_WITHOUT_M1",name_id);
name = "bla";
theStruct = parseXML(sprintf('results_6.xml'));
rb_ids=[101,100,102,103,104,105,106];
fileID = fopen(sprintf('List_Of_Commands_%s.txt',name), 'w');
rbmax=str2double(theStruct.Children(2).Children(8).Children.Data);

nx=str2double(theStruct.Children(2).Children(2).Children.Data);
ny=str2double(theStruct.Children(2).Children(4).Children.Data);
nz=str2double(theStruct.Children(2).Children(6).Children.Data);

addd=nx*ny*nz-3*3*3;   
    
d=theStruct.Children(2).Children(16).Children(2).Children(12).Children(2).Children.Data;
f=theStruct.Children(2).Children(10).Children.Data;
fixedstr = strsplit(f,',');
%%
str = strsplit(d,',');
figure
[a,b,c]=draw(fixedstr,str,nx,ny,nz,rbmax);
%%
for i=1:rbmax
a{i}
b{i}
end 
pause
%%
acm_place=zeros(1,rbmax);
acm0=zeros(1,rbmax);
acm1=zeros(1,rbmax);

for rb=1:rbmax
    
acm_place(rb)=69+addd+(rb-1)*43;
acm0(rb)=str{acm_place(rb)};
acm1(rb)=str{acm_place(rb)+1};

m0_place(rb)=66+addd+(rb-1)*43;
m1_place(rb)=m0_place(rb)+1;
m2_place(rb)=m1_place(rb)+1;

m0(rb)=str2double(str{m0_place(rb)});
m1(rb)=str2double(str{m1_place(rb)});
m2(rb)=str2double(str{m2_place(rb)});

end 

for i=2:floor(length(theStruct.Children(2).Children(16).Children(2).Children(12).Children)/2)
d=theStruct.Children(2).Children(16).Children(2).Children(12).Children(i*2).Children.Data;
str = strsplit(d,',');
fprintf(fileID,'PAUSE \n');
for rb=1:rbmax
%Module rb
if( str{acm_place(rb)}-acm0(rb)~=0)
acm0(rb)=str{acm_place(rb)};
if(str{acm_place(rb)}=='0')
    fprintf(fileID,'sX0-%dpos0\n\n',rb_ids(rb));
else 
    fprintf(fileID,'sX0-%dpos50\n\n',rb_ids(rb));
end
end 
if( str{acm_place(rb)+1}-acm1(rb)~=0)
acm1(rb)=str{acm_place(rb)+1};
if(str{acm_place(rb)+1}=='0')
    fprintf(fileID,'sX1-%dpos0\n\n',rb_ids(rb));
else 
    fprintf(fileID,'sX1-%dpos50\n\n',rb_ids(rb));
end

end 
if( str2double(str{m0_place(rb)})-(m0(rb))~=0)
    m0(rb)=str2double(str{m0_place(rb)});
    fprintf(fileID,'sM0-%dpa%s0\n\n',rb_ids(rb),str{m0_place(rb)});
end 
if( str2double(str{m1_place(rb)})-(m1(rb))~=0)
    m1(rb)=str2double(str{m1_place(rb)});
    fprintf(fileID,'sM1-%dpa%s0\n\n',rb_ids(rb),str{m1_place(rb)});
end 
if( str2double(str{m2_place(rb)})-(m2(rb))~=0)
    m2(rb)=str2double(str{m2_place(rb)});
    fprintf(fileID,'sM2-%dpa%s0\n\n',rb_ids(rb),str{m2_place(rb)});
end 
end 
clf 
[a,b,c,occ]=draw(fixedstr,str,nx,ny,nz,rbmax);

i
pause (0.5)

end 
fclose(fileID);
end 
function [ceil,fl,w1,w2,w3,w4]=string_to_fixedSupport(str,nx,ny,nz)
count=1;
for i=1:nx
    for j=1:ny
        ceil(i,j)=str2double(str{j+(i-1)*ny});
        count=count+1;
    end 
end

for i=1:nx
    for j=1:ny
        fl(i,j)=str2double(str{nx*ny+j+(i-1)*ny});
        count=count+1;
    end 
end

for i=1:nz
    for j=1:ny
        w1(i,j)=str2double(str{2*nx*ny+j+(i-1)*nx});
        count=count+1;
    end 
end

for i=1:nz
    for j=1:nx
        w2(i,j)=str2double(str{nz*ny+2*nx*ny+j+(i-1)*nx});
        count=count+1;
    end 
end

for i=1:nz
    for j=1:ny
        w3(j,i)=str2double(str{nz*ny+nz*nx+2*nx*ny+j+(i-1)*nx});
        count=count+1;
    end 
end

for i=1:nz
    for j=1:nx
        w4(j,i)=str2double(str{2*nz*ny+nz*nx+2*nx*ny+j+(i-1)*nx});
        count=count+1;
    end 
end

end 
function [Occupancy,H_ACM0,H_ACM1,vx0,vx1,motors,acm,heur,cost]=string_to_state(str,nx,ny,nz,rbmax)
count=0;
for k=1:nz
    for i=1:ny
        for j=1:nx
            Occupancy(j,i,k)=str2double(str{(k-1)*nx*ny+j+(i-1)*nx});
            count=count+1;
        end 
    end 
end 
for rb=1:rbmax
    for i=1:4
        for j=1:4            
            H_ACM0{rb}(i,j)=str2double(str{nx*ny*nz+(i-1)*4+j+(rb-1)*43});
            count=count+1;
        end 
    end

    for i=1:4
        for j=1:4 
            H_ACM1{rb}(i,j)=str2double(str{nx*ny*nz+16+(i-1)*4+j+(rb-1)*43});
            count=count+1;
        end 
    end
    
    for i=1:3
        
        vx0(rb,i)=str2double(str{nx*ny*nz+16+16+i+(rb-1)*43}); 
        count=count+1;
    end
    for i=1:3
        
        vx1(rb,i)=str2double(str{nx*ny*nz+16+16+3+i+(rb-1)*43}); 
        count=count+1;
    end
    
    for i=1:3
        motors(rb,i)=str2double(str{nx*ny*nz+16+16+3+3+i+(rb-1)*43});
        count=count+1;
    end
    for i=1:2
        acm(rb,i)=str2double(str{nx*ny*nz+16+16+3+3+3+1+(rb-1)*43+i-1});
        count=count+1;
    end 
end

heur=str2double(str{end-1});
cost=str2double(str{end});
end


function draw_floor(which_w,floor0)
[n1x,n2y]=size(floor0)
z=0;
if which_w==1
    z=4;
end 
for i=1:n1x
    for j=1:n2y
        if(floor0(i,j)==1)
    center=2*[i-1,j-1,z];
    x_direction=[1 0 0];
    y_direction=[0 1 0];
    xy_direction=(x_direction+y_direction);
    rectangle=([center-xy_direction;center+x_direction-y_direction;center+xy_direction;center+y_direction-x_direction])/2;
    patch('XData',rectangle(:,1),'YData',rectangle(:,2),'ZData',rectangle(:,3),'FaceColor','blue','FaceAlpha',0.1)
        end 
    end 
end 
end 
function [H_ACM0,H_ACM1,motors,cost,heur]=draw(strf,str,nx,ny,nz,rbmax)

[Occupancy,H_ACM0,H_ACM1,Vx0,Vx1,motors,acm,heur,cost]=string_to_state(str,nx,ny,nz,rbmax);
g.x=0:nx-1;
g.y=0:ny-1;
g.z=0.5:nz-0.5;
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.2;      %default transparency is 0.2
[ceil,fl,w1,w2,w3,w4]=string_to_fixedSupport(strf,nx,ny,nz);
draw_floor(0,fl);
hold on 
draw_floor(1,ceil);
draw_floor(1,w1);
draw_floor(1,w2);
draw_floor(1,w3);
draw_floor(1,w4);
plotVoxels(Occupancy,g,d,c,alpha);

view(150,20); 
xlim([-1,nx+1]);
ylim([-1,ny+1]);
zlim([-1,nz+1]);
xlabel('x')
ylabel('y')
hold on 
colors={'red';'blue';'green';'yellow';'cyan';'white';'black'};
%plot3(H_ACM0{1}(1,4),H_ACM0{1}(2,4),H_ACM0{1}(3,4),'r*');
for rb=1:rbmax
    center=[H_ACM0{rb}(1,4),H_ACM0{rb}(2,4),H_ACM0{rb}(3,4)];
    x_direction=H_ACM0{rb}(1:3,1)';
    y_direction=H_ACM0{rb}(1:3,2)';
    xy_direction=(x_direction+y_direction);
    rectangle=([center-xy_direction;center+x_direction-y_direction;center+xy_direction;center+y_direction-x_direction])/2;
    patch('XData',rectangle(:,1),'YData',rectangle(:,2),'ZData',rectangle(:,3),'FaceColor',colors{rb})
if(acm(rb,1)==1)
    plot3(center(1)/2,center(2)/2,center(3)/2,'m*','LineWidth',4);
    %rectangle=([center-xy_direction/2;center+x_direction/2-y_direction/2;center+xy_direction/2;center+y_direction/2-x_direction/2]);
    %patch('XData',rectangle(:,1)/2,'YData',rectangle(:,2)/2,'ZData',rectangle(:,3)/2,'FaceColor','magenta')
end 
    center=[H_ACM1{rb}(1,4),H_ACM1{rb}(2,4),H_ACM1{rb}(3,4)];
    x_direction=H_ACM1{rb}(1:3,1)';
    y_direction=H_ACM1{rb}(1:3,2)';
    xy_direction=(x_direction+y_direction);
    rectangle=([center-xy_direction;center+x_direction-y_direction;center+xy_direction;center+y_direction-x_direction])/2;
    patch('XData',rectangle(:,1),'YData',rectangle(:,2),'ZData',rectangle(:,3),'FaceColor',colors{rb})
    if(acm(rb,2)==1)
        plot3(center(1)/2,center(2)/2,center(3)/2,'m*','LineWidth',4);
    end
end 
acm

end
