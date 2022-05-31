      % Prepare data for box_plot grouped by two variables
  
      % load data
      % (requires Statistics or Machine Learning Toolbox)
      load plot_data

      % arrange data
      [y,x,g] = iosr.statistics.tab2box(char(['Simulated annealing'.*ones(1,31)';'Random Search      '.*ones(1,33)']),[duration';durationrbs'],[namesa;namerb]);

      % plot
      figure
      h = iosr.statistics.boxPlot(x,y,...
          'boxColor','auto','medianColor','k',...
          'scalewidth',true,'xseparator',true,...
          'groupLabels',g,'showLegend',true, 'symbolMarker','+');
      box on
      title('Convergence time comparison')
      ylabel('Convergence time (s)')