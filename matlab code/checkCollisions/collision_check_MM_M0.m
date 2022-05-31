%% Read collision
clear all 
%clc 
close all 

figure_on=true;
acm=0;

pos=[];
for z=-2:2:2
    for y=-3:3
        for x=-3:3
            pos=[pos; [x y z]];    
        end 
    end 
end 

b=zeros(7,7,7);
%vexel options 
g.x=[0,1,2,3,4,5,6];
g.y=[0,1,2,3,4,5,6];
g.z=[1/2,1.5,2.5,3.5,4.5,5.5,6.5];
d=[1 1 1];      %default length of side of voxel is 1
c='k';          %default color of voxel is black
alpha=0.3;      %default transparency is 0.2

RB_end{1}=[0 0 0];
end_pos_log=true;
i=0;
end_pos=1;
config=1;
add=[ 0 0 0; 0 0 0; 0 0 1 ; 0 0 1; 0 0 0; 0 0 1];
for name=1:2:4
    
filename = sprintf('./MMcollisionData/logUI_acm%d_m1_P%d.txt',acm,name);%_m1__P_%d.txt',name);%logUI_acm1_P__%d.txt',name);%sprintf('./MMcollisionData/logUI_acm%d_P%d.txt',acm,name);
fID = fopen(filename);
log=true;
q=true;

while q && (~feof(fID))  % Loop until we get to the end of the file
    tline = fgetl(fID);
    if contains(tline,'Plugin exit')
        q=false;
     
    elseif contains(tline,'New Config M')
        log=false;
        end_pos_log=false;
        
    elseif contains(tline,'Start new config')%'New Config M02')
        log=true;
        if figure_on && i>0
        figure 
        plotVoxels(b,g,d,'r',alpha)
        view(150,20); 
        xlim([-1,7]);
        ylim([-1,7]);
        zlim([0,7]);
        b=zeros(7,7,7);
        for k=1:4
            b(RB{config,i}(k,1)+4,RB{config,i}(k,2)+4,RB{config,i}(k,3)+4)=1;
        end 
        plotVoxels(b,g,d,'b',1)

        b=zeros(7,7,7); 
        for kk=1:4
            b(RB_end{config,end_pos}(kk,1)+4,RB_end{config,end_pos}(kk,2)+4,RB_end{config,end_pos}(kk,3)+4)=1;
        end 
        plotVoxels(b,g,d,'g',1);
        end 

        config=config+1;
        i=0;
        end_pos=1;
        RB_end{config,1}=[0,0,0];
if figure_on
     config
     pause 
     close all 
end 
%     
    elseif log && i<1 && contains(tline,'collision detected (stay): ')
        formatspec = sprintf('collision detected (stay): %%d , RB_M%%d' );
        rb=sscanf(tline, formatspec);
        if (rb(1)>=200 && rb(2)~=4)
            newrow=[pos(rb(1)+1-200,:)+add(rb(2)+1,:)];
            if ~(ismember(RB_end{config,1},newrow,'rows'))
                RB_end{config,1}=[RB_end{config,1}; newrow];
                b(newrow(1)+4,newrow(2)+4,newrow(3)+4)=1;
            end 
        end 
     
    
     elseif log && contains(tline,'Plugin --> New Angle ')
        end_pos_log=false;
        RB{config,i+1}=RB_end{config,end_pos};
        if figure_on && i>0
            figure 
            plotVoxels(b,g,d,'r',alpha)
            view(150,20); 
            xlim([-1,7]);
            ylim([-1,7]);
            zlim([0,7]);
            xlabel('x');
            ylabel('y');
            b=zeros(7,7,7);           
            for k=1:4
                b(RB{config,i}(k,1)+4,RB{config,i}(k,2)+4,RB{config,i}(k,3)+4)=1;
            end 
            plotVoxels(b,g,d,'b',1);
            b=zeros(7,7,7); 
            for kk=1:4
                b(RB_end{config,end_pos}(kk,1)+4,RB_end{config,end_pos}(kk,2)+4,RB_end{config,end_pos}(kk,3)+4)=1;
            end 
            plotVoxels(b,g,d,'g',1);
        end 
        b=zeros(7,7,7);
        formatspec = sprintf('Plugin --> New Angle M%%d from %%d to %%d' );
        i=i+1;
        motor{config,i}=sscanf(tline, formatspec);
        

     
    elseif log && i>0 && contains(tline,'collision detected:')
        formatspec = sprintf('collision detected: %%d , RB_M%%d' );
        rb=sscanf(tline, formatspec);
        if rb(1)>=200
            newrow=[pos(rb(1)+1-200,:)+add(rb(2)+1,:)];
            if ~any(ismember(RB{config,i},newrow,'rows'))
                RB{config,i}=[RB{config,i}; newrow];
                b(newrow(1)+4,newrow(2)+4,newrow(3)+4)=1;
            end 
        end 
     
     elseif log && contains(tline,'Plugin --> End of action')
        end_pos=end_pos+1;
        RB_end{config,end_pos}=[0 0 0];
        end_pos_log=true;
        
    
    elseif log && end_pos>1 && end_pos_log && contains(tline,'collision detected (stay):')
        formatspec = sprintf('collision detected (stay): %%d , RB_M%%d' );
        rb=sscanf(tline, formatspec);
        if rb(1)>=200 && rb(2)~=4
            newrow=[pos(rb(1)+1-200,:)+add(rb(2)+1,:)];
            if ~any(ismember(RB_end{config,end_pos},newrow,'rows'))
                RB_end{config,end_pos}=[RB_end{config,end_pos}; newrow];
                b(newrow(1)+4,newrow(2)+4,newrow(3)+4)=1;
            end 
        end 
    end 
end

        config=config+1;
        i=0;
        end_pos=1;
        RB_end{config,1}=[0,0,0];
end 

%%  Write to file 
acm=1;
different_orientations=[0 0 0 ;
                        0  pi/2 0 ;
                        -pi/2 0 pi/2;
                        0 -pi/2 0;
                        pi/2 0 -pi/2;
                        pi 0 -pi];%[z,x,y,-x,-z,-y]

yaw=[0,pi/2,pi,-pi/2];                    

fileID0 = fopen(sprintf('MM_Voxel_Occupied_M0_%d.txt',acm), 'w');
fileID1 = fopen(sprintf('MM_Voxel_Occupied_M1_%d.txt',acm), 'w');
s='';
skipped=[];
[c2,c1]=size(RB);
keys=[];

if acm==1
    init=eul2tform([0 0 yaw(3)],'XYZ');
else 
    init=eye(4);
end 
for or=1:1
    init_orientation1=eul2tform(different_orientations(or,:),'XYZ');        
    for y=1:4
        init_orientation=init*init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
        init_orientations=init_orientation1*eul2tform([0 0 yaw(y)],'XYZ');
        for conf1=1:c1
            for conf2=1:c2
                if(motor{conf2,conf1}(1)==0)
                    fileID=fileID0;
                    orient=[acm init_orientation(1,1:3) init_orientation(2,1:3) init_orientation(3,1:3) motor{conf2,conf1}(3)-motor{conf2,conf1}(2)];

                else
                    fileID=fileID1;
                    orient=[acm init_orientations(1,1:3) init_orientations(2,1:3) init_orientations(3,1:3) motor{conf2,conf1}(3)-motor{conf2,conf1}(2)];

                end 
%                if conf1>1
                 %                else
%                    orient=[acm init_orientation(1,1:3) init_orientation(2,1:3) init_orientation(3,1:3) 0 motor{conf2,conf1}(3)];
                poses=[];
                for i=2:4
                    new=round(init_orientation*[RB_end{conf2,conf1}(i,:) 1]');
                    new(4)=norm(new(1:3));
                    poses=[poses; new'];
                end

                if(length(unique(poses(:,4))))>2
                    poses=sortrows(poses,4);
                else 
                    skipped=[skipped;[conf2,conf1]];
                    [p,indx]=sortrows(poses,4);
                    if indx(3)==1
                        poses([1 2],:)=poses([2 1],:);
                    elseif indx(3)==3
                        poses([2 3],:)=poses([3 2],:);
                    else 
                        poses
                   %continue;
                    end
                end
                
                pose=reshape(poses(:,1:3)',1,9);
                orient=[orient pose];
                orient=int2str(orient);
                orient(orient == ' ') = [];
                
                if ~any(strcmp(keys,orient))
                    keys{end+1}=orient;
                for i = 5: length(RB{conf2,conf1})
                    new=round(init_orientation*[RB{conf2,conf1}(i,:) 1]');
                    if i==5
                        s=sprintf('%d,%d,%d',new(1),new(2),new(3));
                    else 
                        s=strcat(s,sprintf(',%d,%d,%d',new(1),new(2),new(3)));
                    end 
                end
                fprintf(fileID,'CollisionMM_LookUpTable.Add("%s", new int[] {%s}); \n',orient,s);
                end 
            end 
        end 
    end 
end 
