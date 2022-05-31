load('ceiling_newest.mat');
titre1 = sprintf('\n\n\n\n %s','    Crossing a horizontal gap with 2 modules');
titre2 = sprintf('\n\n\n\n %s','    Ground to ceiling transition with 2 modules');

titre=titre2;
close all 

%%
%% Number of problems solved in 30 mins 
count=zeros(8,2);
for i=1:8
    for j=1:2
        count(i,j)=sum(Solved{i,j});
    end 
end 

%% Number of problems not solved within 30 mins 
count_more_than_30=zeros(8,2);
for i=1:8
    for j=1:2
        count_more_than_30(i,j)=sum(SolutionTime{i,j}>1800000);
    end 
end

%% No solution
count_no_solution=zeros(5,2);
for i=1:8
    for j=1:2
        count_no_solution(i,j)=sum(and(SolutionTime{i,j}<1800000,Solved{i,j}==0));
    end 
end

%% When solution found, what is the mean solution time. 
mean_time=zeros(5,2);
std_time=zeros(5,2);
median_time=zeros(5,2);
for i=1:8
    for j=1:2
        mean_time(i,j)=mean(SolutionTime{i,j}(find(Solved{i,j}==1)))/1000;
        std_time(i,j)=std(SolutionTime{i,j}(find(Solved{i,j}==1)))/1000;
        median_time(i,j)=median(SolutionTime{i,j}(find(Solved{i,j}==1)))/1000;
    end 
end
%% When solution found, what is the mean nodes stored. 
expansion_mean=zeros(8,2);
for i=1:8
    for j=1:2
        %expansion_mean(i,j)=mean(NoOfNodeExpansion{i,j}(find(SolutionTime{i,j}==1)));
         expansion_mean(i,j)=mean(NoOfNodeExpansion{i,j}(find(SolutionTime{i,j}<1000*60*30)));
        
    end
end

%% Solved by the algorihtm: all instances where solutiontime is < 30 mins 
solved_count=zeros(8,2);
for i=1:8
    for j=1:2
        %expansion_mean(i,j)=mean(NoOfNodeExpansion{i,j}(find(SolutionTime{i,j}==1)));
         solved_count(i,j)=length(find(SolutionTime{i,j}<1000*60*30));
        
    end
end

%%
figure('rend','painters','pos',[10 10 00 600],'color','w')
w=solved_count([1,6,3,4,8,2],:);%([1,2,3,4,6,8],:);
w(end-1,end)=0;
w(end,end)=0;
b=bar(w)
b(1).CData = [0,0,1];
b(2).CData = [0,1,0];
names = {'A*';'A* nah';'BFS';'DFS';'Bi-D A*';'Bi-D A* nah'};
set(gca,'xtick',[1:6],'xticklabel',names)
%xlabel('Algorithms')
ylabel('Number of solved problems (within 30 mins)')
title(titre,'interpreter','latex',...
         'HorizontalAlignment', 'right')
%fix_xticklabels(gca,0.1,{'FontSize',10});
hold on 
w=count([1,6,3,4,8,2],:);%([1,2,3,4,6,8],:);
w(end-1,end)=0;
w(end,end)=0;
b=bar(w,'FaceColor','flat')
for i=1:length(w)
    for j=1:2
      try
          if j==1
        xl = [0.743+i-1 0.97+i-1] ; yl = [0 w(i,j)] ;
        c='b';
          else 
        xl = [1.03+i-1 1+(1-0.743)+i-1] ; yl = [0 w(i,j)] ;
        c= [ 0.9100 0.4100 0.1700];
          end 
        [X,Y] = hatch_coordinates( xl , yl,xl(2)-xl(1),3,true) ;
        a=isnan(X);
        X(find(a))=[];
        Y(find(a))=[];
        hold on 
        for k=1:2:length(X)
           line(X(k:k+1),Y(k:k+1),'color',c)
           hold on
        end
      catch 
      end 
    end 
end 
b(1).CData = [0.73,0.83,0.96];
b(2).CData = [0.95,0.87,0.73];
legend1=legend('with orientation, solution doesn''t exist','without orientation, solution doesn''t exist','with orientation, solution exists','without orientation, solution exists');
set(gcf, 'Position',  [600 350 500 250]);
set(gca, 'FontName', 'Times','FontSize',6)
%legend1 = legend(axes1,'show');
set(legend1,...
    'Position',[0.563833333333334 0.827850260606554 0.340666666666667 0.160666666666667]);
export_fig ../../ICRA2019/figures/pdf/comparison_bar1.pdf
saveas(gca,'../../ICRA2019/figures/fig/comparison_bar1.fig');


%% convergence time
figure('rend','painters','pos',[10 10 800 600])
w=mean_time([1,6,8,2,3,4],:);%([1,2,3,4,6,8],:);
w(3,2)=0;
w(4,2)=0;
%w(end,end)=0;
bar(w)
names = {'A star (admissible)';'A star (not-admissible)';'Bidirectional A star (admissible)';'Bidirectional A star (not-admissible)';'BFS';'DFS';'Bidirectional BFS'};
set(gca,'xtick',[1:6],'xticklabel',names)
%xlabel('Algorithms')
ylabel('Mean convergence time (in seconds)')
legend('with orientation','without orientation','Location','best')
title(titre)
fix_xticklabels(gca,0.3,{'FontSize',8});
set(gca, 'FontName', 'Times')
axis tight
%% steps
% figure
% w=mean_step([1,3,4,6],:);
% w=w([1,4,2,3],:);
% bar(w)
% hold on
% names = {'A* (admissible)';'A* (Not-admissible)';'BFS';'DFS'};
% set(gca,'xtick',[1:5],'xticklabel',names)
% %xlabel('Algorithms')
% ylabel('Mean path cost')
% legend('with orientation','without orientation','Location','best')
% title(titre)
% ylim([0,35])
% fix_xticklabels(gca,0.1,{'FontSize',10});

%% expanded nodes
figure('rend','painters','pos',[10 10 800 600])
w=expansion_mean([1,6,8,2,3,4],:);%([1,2,3,4,6,8],:);
w(3,2)=0;
w(4,2)=0;
%w(end,end)=0;
bar(w)
hold on
names = {'A star (admissible)';'A star (not-admissible)';'Bidirectional A star (admissible)';'Bidirectional A star (not-admissible)';'BFS';'DFS';'Bidirectional BFS'};
set(gca,'xtick',[1:6],'xticklabel',names)
%xlabel('Algorithms')
ylabel('Mean number of expansion nodes')
legend('with orientation','without orientation','Location','best')
title(titre)
fix_xticklabels(gca,0.1,{'FontSize',10});

%% when optimal exist compare optimality...  
optimal_index=find(Solved{1,1});
the_optimal_steps=Steps{1,1}(optimal_index);
figure('rend','painters','pos',[10 10 800 600])
solved_index=Solved{1,1}(optimal_index);
for i=1:8
    for j=1:2
        a=find(Solved{i,j}(optimal_index));
        count_not_f(i,j)=length(a)
        the_steps=Steps{i,j}(optimal_index)
        steps=the_steps(a);
        optimal_steps=the_optimal_steps(a)
        b=steps-optimal_steps
        optimal_steps(find(b<0))=[]
        b(find(b<0))=[]     
        optimality(i,j)=mean((b)./optimal_steps);
        if isnan(optimality(i,j))
            optimality(i,j)=0;
        end 
        optimality(i,j)=100-100*optimality(i,j);
        if(optimality(i,j)<0)
            optimality(i,j)=1;
        end 
    end 
end
w=optimality([1,6,8,2,3,4],:);
w(3,2)=0;
w(4,2)=0;
bar(w)%,5,7]));
       names = {'A star (admissible)';'A star (not-admissible)';'Bidirectional A star (admissible)';'Bidirectional A star (not-admissible)';'BFS';'DFS'};%'Multidirectional';'Bidirectional BFS'};
set(gca,'xtick',[1:6],'xticklabel',names)
%xlabel('Algorithms')
ylabel('Optimality percentage')
legend('with orientation','without orientation','Location','best')
title(titre)
fix_xticklabels(gca,0.3,{'FontSize',10});
%%
k=[1,6,3,4];
count_not_f=[];
for i=k
    for j=2:2
        optimal_index=find(Solved{i,1});
        a=find(Solved{i,j}(optimal_index));
        count_not_f=[count_not_f,100-(count(i,1)-length(a))];
    end 
end 
figure('rend','painters','pos',[10 10 800 600])
count_not=[[100 100 100 100]',count_not_f'];
bar(count_not)
hold on
names = {'A star (admissible)';'A star (not-admissible)';'BFS';'DFS'};
set(gca,'xtick',[1:4],'xticklabel',names)
%xlabel('Algorithms')
ylabel('Completeness Percentage')
%legend('with orientation','without orientation','Location','best')
title(titre)
fix_xticklabels(gca,0.3,{'FontSize',10});
ylim([0 100])
legend('with orientation','without orientation','Location','best')

% %%
% for i=1:8
%     for j=1:1
%         optimal_index=find(Solved{i,2});
%         a=find(Solved{i,1}(optimal_index));
%         count_not_f(i,j+1)=count(i,2)-length(a);
%     end 
% end 

%% simple hatch
% %// simple hatch, angle changed
% [X,Y] = hatch_coordinates( xl , yl , 0.2 ) ;
% subplot(1,4,1) ; plot(X,Y) ; grid off
% 
% %// heavy line hatching
% [X,Y] = hatch_coordinates( xl , yl , 0.5 ) ;
% subplot(1,4,2) ; plot(X,Y,'k','linewidth',2) ;grid off
% 
% %// very light color hatching, flatter angle, dotted lines
% [X,Y] = hatch_coordinates( xl , yl , 1 , 0.1 ) ;
% subplot(1,4,3) ; plot(X,Y,'Color',[.7 .7 .7],'linewidth',1,'LineStyle',':') ;grid off
% 
% %// multi color hatching, (specify option "merge=false" )
% [X,Y] = hatch_coordinates( xl , yl , 0.5 , 0.5 , false ) ;
% subplot(1,4,4) ; plot(X,Y) ;grid off