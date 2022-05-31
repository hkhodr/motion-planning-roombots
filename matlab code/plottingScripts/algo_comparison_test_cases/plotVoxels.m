function plotVoxels(b,g,d,c,alpha)

%plotVoexels function to draw large number of 3-D voxels in a 3-D plot.
%
%Usage
%   plotVoxels(binary3Dgrid,voxelcoordinates,voxelsize,color,alpha);
%
%   will draw a voxels which are given in 'binary3Dgrid' at 
%   'voxelcoordinates' of size 'voxelsize' of color 'color' and
%   transparency 'alpha' 
%   Default size is 1
%   Default color is blue
%   Default alpha value is 0.8
%
%   binary3Dgrid: a binary 3D matrix of size (M,N,P) that represents the whole space in 3D grid of size MxNxP. 0 means no voxel, 1 means voxel. 
%   voxelcoordinates: a struct with three vectors (.x .y .z). Each array holds real coordinates of voxels (given by binary3Dgrid) in x, y, z axes. 
%   voxelsize: a three element vector [dx,dy,dz]. voxel size along different axes can be different.
%   color: a character string to specify color (type 'help plot' to see list of valid colors)
%   alpha is the transparancy of voxels. 1 for opaque, 0 for transparent
%
%   For demostration (or click the run button and rotate the plotted figure):
%   plotVoxels();
%   view(3)
%
%   Tested on Matlab 2015a

%   Mehmet Mutlu, Jan 20,2017. 
%   Acknowledgements: 
%   Inspired my Suresh Joel's voxel.m and 
%   Itzik Ben Shabat's question on Matlab forums
%   http://www.mathworks.com/matlabcentral/fileexchange/3280-voxel
%   https://www.mathworks.com/matlabcentral/answers/55633-how-to-drawing-3d-voxel-data

switch(nargin)
case 0
    b=ones(2,2,3);
    b(1,1,1)=0;
    g.x=[0,1];
    g.y=[3,4];
    g.z=[7,8,9];
    d=[1 1 1];      %default length of side of voxel is 1
    c='b';          %default color of voxel is blue
    alpha=0.8;      %default transparency is 0.8
    disp('Too few arguements for voxel, drawing example 2x2x3 voxel grid with singe missing voxel')
case 1
    disp('Too few arguements for voxel')
    return;
case 2
    d=[1 1 1];      %default length of side of voxel is 1
    c='b';          %default color of voxel is blue
    alpha=0.8;      %default transparency is 0.8
case 3
    c='b';          %default color of voxel is blue
    alpha=0.8;      %default transparency is 0.8
case 4
    alpha=0.8;      %default transparency is 0.8
case 5
    %do nothing
otherwise
    disp('Too many arguements for voxel')
end


numberOfVox = numel(find(b));

FV.vertices=zeros(numberOfVox*8,3);
FV.faces=zeros(numberOfVox*6,4);

cnt=0; 

% plot voxels
for i=1:length(b(:,1,1))
   for j=1:length(b(1,:,1))
      for k=1:length(b(1,1,:)) 
        if b(i,j,k)==1           
            FV_new.vertices=[g.x(i)+[-d(1)/2 d(1)/2 d(1)/2 -d(1)/2 -d(1)/2 d(1)/2 d(1)/2 -d(1)/2]; ...
            g.y(j)+[-d(2)/2 -d(2)/2 -d(2)/2 -d(2)/2 d(2)/2 d(2)/2 d(2)/2 d(2)/2]; ...
            g.z(k)+[-d(3)/2 -d(3)/2 d(3)/2 d(3)/2 -d(3)/2 -d(3)/2 d(3)/2 d(3)/2]]';
        
            FV_new.faces=[1 2 3 4; 2 6 7 3 ; 6 5 8 7; 5 1 4 8; 4 3 7 8 ; 1 2 6 5];
            FV_new.faces=FV_new.faces+cnt*8;
            FV.vertices(cnt*8+1:cnt*8+8,:)=FV_new.vertices(:,:);
            FV.faces(cnt*6+1:cnt*6+6,:)=FV_new.faces(:,:);
            cnt=cnt+1;
        end
      end
   end
end

patch(FV,'FaceColor',c,'FaceAlpha',alpha);

end