%xml to list of commands
close all;
clear all; 
clc ;

titl = sprintf('Crossing a horizontal gap \n with 2 modules');
%titl = sprintf('Ground to ceiling transition \n with 2 modules');

theStruct = parseXML(sprintf('bridge.xml'));%./results_xml/rb2 without/results_%s.xml',name));
rb_ids=[100,101,102,103,104,105,106,107,108,109,110,111,112];
rbmax=str2double(theStruct.Children(2).Children(8).Children.Data);
nx=str2double(theStruct.Children(2).Children(2).Children.Data);
ny=str2double(theStruct.Children(2).Children(4).Children.Data);
nz=str2double(theStruct.Children(2).Children(6).Children.Data);
addd=nx*ny*nz-3*3*3;      
goal=theStruct.Children(2).Children(14).Children.Data;
f=theStruct.Children(2).Children(10).Children.Data;
fixedstr = strsplit(f,',');
%%
str = strsplit(goal,',');
axis equal 
[a,b,c]=draw(fixedstr,str,nx,ny,nz,rbmax,titl);

%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%%55
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
        w1(i,j)=str2double(str{2*nx*ny+j+(i-1)*ny});
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
        w3(j,i)=str2double(str{nz*ny+nz*nx+2*nx*ny+j+(i-1)*ny});
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
    order(rb)=str2double(str{nx*ny*nz+1+(rb-1)*44});
    %order(rb)=rb-1;
    for i=1:4
        for j=1:4            
            H_ACM0{order(rb)+1}(i,j)=str2double(str{nx*ny*nz+(i-1)*4+j+(rb-1)*44+1});
            count=count+1;
        end 
    end

    for i=1:4
        for j=1:4 
            H_ACM1{order(rb)+1}(i,j)=str2double(str{nx*ny*nz+16+(i-1)*4+j+(rb-1)*44+1});
            count=count+1;
        end 
    end
    
    for i=1:3
        
        vx0(order(rb)+1,i)=str2double(str{nx*ny*nz+16+16+i+(rb-1)*44+1}); 
        count=count+1;
    end
    for i=1:3
        
        vx1(order(rb)+1,i)=str2double(str{nx*ny*nz+16+16+3+i+(rb-1)*44+1}); 
        count=count+1;
    end
    
    for i=1:3
        motors(order(rb)+1,i)=str2double(str{nx*ny*nz+16+16+3+3+i+(rb-1)*44+1});
        count=count+1;
    end
    for i=1:2
        
        acm(order(rb)+1,i)=str2double(str{nx*ny*nz+16+16+3+3+3+1+(rb-1)*44+i-1+1});
        count=count+1;
    end 
end


heur=str2double(str{end-1});
cost=str2double(str{end});
end


function draw_floor(which_w,floor0,nx,ny,nz)
switch(which_w)
    case 0
        x_vec=0:nx-1;
        y_vec=0:ny-1;
        z_vec=0:0;
        x_direction=[1 0 0];
        y_direction=[0 1 0];
        e=[1,1,0];
    case 1
        x_vec=-1/2;
        y_vec=0:ny-1;
        z_vec=1/2:nz-1/2;
        x_direction=[0 0 1];
        y_direction=[0 1 0]; 
        e=[0,1,1/2];
    case 2
        y_vec=-1/2;
        x_vec=0:nx-1;
        z_vec=1/2:nz-1/2;
        x_direction=[0 0 1];
        y_direction=[1 0 0];
        e=[1,0,1/2];
    case 3
        x_vec=nx-1/2;
        y_vec=0:ny-1;
        z_vec=1/2:nz-1/2;
        x_direction=[0 0 1];
        y_direction=[0 1 0];
        e=[0,1,1/2];
    case 4
        y_vec=ny-1/2;
        x_vec=0:nx-1;
        z_vec=1/2:nz-1/2;
        x_direction=[0 0 1];
        y_direction=[1 0 0];
        e=[1,0,1/2];
    case 5
        x_vec=0:nx-1;
        y_vec=0:ny-1;
        z_vec=nz:nz;
        x_direction=[1 0 0];
        y_direction=[0 1 0];
        e=[1,1,0];
        
end 
for i=x_vec
    for j=y_vec
        for k=z_vec
            center=2*[i,j,k];
            which_x=find(e);
            if(all(which_x==[1 2]))
                x=i+e(which_x(1));
                y=j+e(which_x(2));
            elseif (all(which_x==[2 3]))
                x=j+e(which_x(1));
                y=k+e(which_x(2));
            elseif (all(which_x==[1 3]))
                x=i+e(which_x(1));
                y=k+e(which_x(2));
            else
                x=-100
                y=-100
            end 

            if(floor0(x,y)==1)
                color='blue';
            
            xy_direction=(x_direction+y_direction);
            rectangle=([center-xy_direction;center+x_direction-y_direction;center+xy_direction;center+y_direction-x_direction])/2;
            patch('XData',rectangle(:,1),'YData',rectangle(:,2),'ZData',rectangle(:,3),'FaceColor',color,'FaceAlpha',0.1)
            else
                
                color='red';
            end 
            end 
    end
end 
end

function [H_ACM0,H_ACM1,motors,cost,heur]=draw(strf,str,nx,ny,nz,rbmax,titl)

[Occupancy,H_ACM0,H_ACM1,Vx0,Vx1,motors,acm,heur,cost]=string_to_state(str,nx,ny,nz,rbmax);
g.x=0:nx-1;
g.y=0:ny-1;
g.z=0.5:nz-0.5;
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2
[ceil,fl,w1,w2,w3,w4]=string_to_fixedSupport(strf,nx,ny,nz);
draw_floor(0,fl,nx,ny,nz);
hold on 
draw_floor(1,w1',nx,ny,nz);
draw_floor(2,w2',nx,ny,nz);
draw_floor(3,w3,nx,ny,nz);
draw_floor(4,w4,nx,ny,nz);
draw_floor(5,ceil,nx,ny,nz);
plotVoxels(Occupancy,g,d,c,alpha); 
colors={'red';'blue';'green';'yellow';'cyan';'white';'black';'magenta';'red';'blue';'green';'yellow';'cyan';'white';'black';'magenta'};
%plot3(H_ACM0{1}(1,4),H_ACM0{1}(2,4),H_ACM0{1}(3,4),'r*');
for rb=1:rbmax
    center=[H_ACM0{rb}(1,4),H_ACM0{rb}(2,4),H_ACM0{rb}(3,4)];
    x_direction=H_ACM0{rb}(1:3,1)';
    y_direction=H_ACM0{rb}(1:3,2)';
    xy_direction=(x_direction+y_direction);
    rectangle=([center-xy_direction;center+x_direction-y_direction;center+xy_direction;center+y_direction-x_direction])/2;
    patch('XData',rectangle(:,1),'YData',rectangle(:,2),'ZData',rectangle(:,3),'FaceColor',colors{rb})
if(acm(rb,1)==1)
    plot3(center(1)/2,center(2)/2,center(3)/2,'m*','LineWidth',2);
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
        plot3(center(1)/2,center(2)/2,center(3)/2,'m*','LineWidth',2);
    end
end 
set(gca, 'FontName', 'Times')
title(titl);
view([-33 50])

axis tight
zlim([0 4]);
xlim([-0.5 7.5]);
ylim([-0.5 5.5]);
xlabel('x');
ylabel('y');
zlabel('z');
end
