%M0 self collision: 

clear all 
clc 
close all 

%%
figure_on=false;
acm=1;
b=zeros(7,7,7);
b(4,4,4)=1;
%vexel options 
g.x=[0,1,2,3,4,5,6];
g.y=[0,1,2,3,4,5,6];
g.z=[1/2,1.5,2.5,3.5,4.5,5.5,6.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2

%%  Write to file 
different_orientations=[0 0 0 ;
                        0  pi/2 0 ;
                        -pi/2 0 pi/2;
                        0 -pi/2 0;
                        pi/2 0 -pi/2;
                        pi 0 -pi];%[z,x,y,-x,-z,-y]

yaw=[0,pi/2,pi,3*pi/2];                    
fileID = fopen(sprintf('MM_Voxel_Occupied_SM0_%d.txt',acm), 'w');
s='';
if acm==0
    
    RBPOS={[0 0 1; 1 0 1; 1 0 0],[0,0,1;0 1 1; 0 1 0],[-1 0 0;-1 1 0;0 1 0],[0 -1 0 ; 1 -1 0; 1 0 0],[-1 0 0 ; -1 0 -1 ; 0 0 -1],[0 -1 0 ;0 -1 -1; 0 0 -1]};
else 
    RBPOS={[0 0 1; -1 0 1; -1 0 0],[0,0,1;0 -1 1; 0 -1 0],[1 0 0;1 -1 0;0 -1 0],[0 1 0 ; -1 1 0; -1 0 0],[1 0 0 ; 1 0 -1 ; 0 0 -1],[0 1 0 ;0 1 -1; 0 0 -1]};
end 

keys={};
for or=1:6
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ');        
    for y=1:4   
        b=zeros(7,7,7);
        b(4,4,4)=1;
        init_orientation=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
        
        orientation=[acm init_orientation(1,1:3) init_orientation(2,1:3) init_orientation(3,1:3)];
        
        for k=1:length(RBPOS)
            orient=orientation;
            for i=1:3
                new=round(init_orientation*[RBPOS{k}(i,:) 1]');
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
                pause

            end
        end 
    end
end 
fclose(fileID);
