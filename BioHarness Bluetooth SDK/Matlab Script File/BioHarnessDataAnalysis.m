%********************************************************************************************%
% FileName:     BioHarnessDataAnalysis.m
% Author:       Amit Mukherjee
% Company:      Zephyr Tecnology
% Date:         6/7/2011
% Description:  This file requests the user to input an ECG , a General
%               Packet and a BR_RR csv file for the BioHarness.Upon providing them, it plots the signed
%               ECG data and the Heart & Breathing Rate along with the
%               Acceleration, HRV etc. information in three graphs for the user to
%               analyze.
%**********************************************************************************************%

clear all
close all
%Getting the ECG File******************************************************
[filename, pathname] = uigetfile('*.csv', ' Please select the ECG Input file');
valid_filename=0;
% Searching for the ECG String in the filename
ECG_StrinG_Search = strfind(filename, 'ECG');
if(~(isempty(ECG_StrinG_Search)))
    valid_filename=1;
    disp('Invalid File or No File Selected ');
end
%Do if a file with validname has been found
if(valid_filename)
    %concatenate the pathname with the filename
    CompletePathwFilename = strcat(pathname,filename);
    %Open the file & extract the data
    fid = fopen(CompletePathwFilename);
    data = textscan(fid,'%s %f','HeaderLines',1,'Delimiter',',','CollectOutput',1);
    fclose(fid);
    ADC_Resolution =12;
    Fs_ECG = 250;

    %Obtain the actual ECG data from the cell of arrays and set figure
    %properties
    Actual_ECG_Data = data{1,2};
    time = transpose(0:1/Fs_ECG:length(Actual_ECG_Data)/Fs_ECG-1/Fs_ECG); 
    Actual_ECG_Data_Signed = (Actual_ECG_Data-(2^(ADC_Resolution-1)))*(-1);
     figure('Name','ECG Data','NumberTitle','off')
    plot(time,Actual_ECG_Data_Signed);
    title('Plot of Signed ECG Data');ylabel('Signed ECG Data');xlabel('Time in seconds');
    grid on
end
pause(1);
%Getting the General Packet File******************************************************
[filename, pathname] = uigetfile('*.csv', ' Please select the General Packet Input file');
valid_filename=0;
% Searching for the 'General' String in the filename
GP_StrinG_Search = strfind(filename, 'General');

if(~(isempty(GP_StrinG_Search)))
    valid_filename=1;
    disp('Invalid File or No File Selected File or No File Selected');
end

%Do if a file with validname has been found
if(valid_filename)
    %concatenate the pathname with the filename
    CompleteGPPathwFilename = strcat(pathname,filename);
    %Open the file & extract the data
    fid = fopen(CompleteGPPathwFilename);
    GP_Data = textscan(fid,'%s %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f %f','HeaderLines',1,'Delimiter',',','CollectOutput',1);
    fclose(fid);
      %Obtain the actual GP data from the cell of arrays
    Actual_GP_Data = GP_Data{1,2};

    % Get the Heart Rate data from the extracted data
    HeartRate = Actual_GP_Data(:,1);
    timeGP = 1:1:length(Actual_GP_Data);
    % Set figure properties for the HR and Breathing data
     figure('Name','Heart and Breathing Rate','NumberTitle','off')
    subplot(2,1,1);
    plot(timeGP,HeartRate);
    grid on;
    ylabel('Heart Rate (bpm)');xlabel('Time (seconds)');
    legend('Heart Rate');
    title('Heart and Breathing Rate');
  
    % Get the Breathing Rate data from the extracted data
    BreathingRate = Actual_GP_Data(:,2);
    subplot(2,1,2);
    plot(timeGP,BreathingRate);
    grid on;
    ylabel('Breathing Rate (bpm)');xlabel('Time (seconds)');
    legend('Breathing Rate');
    
   % Set figure properties for the Acceleration data
    figure('Name','Acceleration Data','NumberTitle','off')
    title('Acceleration Data');
    
    % Get the Peak Accelertion data from the extracted data
    % Then plotting the Peak Accelertion data
    Acc_Data = Actual_GP_Data(:,6);
    subplot(4,1,1);
    plot(timeGP,Acc_Data);
    grid on;
    ylabel('Acceleration (g)');xlabel('Time (seconds)');
    legend('Peak Acceleration');
     title('Acceleration Data');
    
     % Get the X-axis Accelertion data from the extracted data
     X_axis_Min_Acc_Data = Actual_GP_Data(:,11);
    X_axis_Peak_Acc_Data= Actual_GP_Data(:,12);
    
    % Then plotting the Peak/Min X-axis Accelertion data
    subplot(4,1,2);
    plot(timeGP,X_axis_Min_Acc_Data,'b',timeGP,X_axis_Peak_Acc_Data,'r');
    grid on;
    ylabel('X-axis Min/Peak Acc.(g)');xlabel('Time (seconds)');
    legend('X-axis Min Acceleration','X-axis Peak Acceleration');
 
       % Get the Y-axis Accelertion data from the extracted data
    Y_axis_Min_Acc_Data = Actual_GP_Data(:,13);
    Y_axis_Peak_Acc_Data= Actual_GP_Data(:,14);
    
    
    % Then plotting the Peak/Min Y-axis Accelertion data
    subplot(4,1,3);
    plot(timeGP,Y_axis_Min_Acc_Data,'b',timeGP,Y_axis_Peak_Acc_Data,'r');
    grid on;
    ylabel('Y-axis Min/Peak Acc.(g)');xlabel('Time (seconds)');
    legend('Y-axis Min Acceleration','Y-axis Peak Acceleration');
    
    

      % Get the Z-axis Accelertion data from the extracted data
    Z_axis_Min_Acc_Data = Actual_GP_Data(:,15);
    Z_axis_Peak_Acc_Data= Actual_GP_Data(:,16);
    % Then plotting the Peak/Min Z-axis Accelertion data
    subplot(4,1,4);
    plot(timeGP,Z_axis_Min_Acc_Data,'b',timeGP,Z_axis_Peak_Acc_Data,'r');
    grid on;
    ylabel('Z-axis Min/Peak Acc.(g)');xlabel('Time (seconds)');
    legend('Z-axis Min Acceleration','Z-axis Peak Acceleration');
end

[filename, pathname] = uigetfile('*.csv', ' Please select the Breathing Rate & R-R Input file');
valid_filename=0;
% Searching for the ECG String in the filename
BR_RR_StrinG_Search = strfind(filename, 'BR_RR');
if(~(isempty(BR_RR_StrinG_Search)))
    valid_filename=1;
    disp('Invalid File or No File Selected File or No File Selected');
end
%Do if a file with validname has been found
if(valid_filename)
    %concatenate the pathname with the filename
    CompletePathwFilename = strcat(pathname,filename);
    %Open the file & extract the data
    fid = fopen(CompletePathwFilename);
    BR_RR_data = textscan(fid,'%s %f %f','HeaderLines',1,'Delimiter',',','CollectOutput',1);
    fclose(fid);
    
    Actual_BR_RR_Data = BR_RR_data{1,2};
    Breathing_Rate_Data = Actual_BR_RR_Data(:,1);
    
    
    Fs_BR_RR = 1/0.056;% R-R/BR data samples are separated by 56ms in time
    timeBR_RR = transpose(0:1/Fs_BR_RR:length(Breathing_Rate_Data)/Fs_BR_RR-1/Fs_BR_RR);
    subplot(3,1,1);
    plot(timeBR_RR,Breathing_Rate_Data);
    title('Breathing and R to R Data');
    ylabel('Breathing Rate (bpm)');xlabel('Time (seconds)');
    legend('Breathing Rate');
    grid on
    
    
    R_R_Data = Actual_BR_RR_Data(:,2);
    subplot(3,1,2);
    plot(timeBR_RR,R_R_Data);
    ylabel('R to R Data');xlabel('Time (seconds)');
    legend('R to R');
    grid on
    
    NumMins =5; SecPerMin =60;
    SecsForRollingAverage =30;
    SamplesforHRV = round(SecsForRollingAverage*Fs_BR_RR);
    FirstHRVTime = NumMins*SecPerMin;
    NumSamplesOverFirstHRV = round(FirstHRVTime*Fs_BR_RR);
    HRV = zeros(1,(length(R_R_Data)-SamplesforHRV-NumSamplesOverFirstHRV)+1);
    if(length(R_R_Data)>NumSamplesOverFirstHRV)
        %Calculate the first HRV for 5 min of data
        for i=0:(length(R_R_Data)-SamplesforHRV-NumSamplesOverFirstHRV);
            HRVMean = sum(R_R_Data(NumSamplesOverFirstHRV+i+3-SamplesforHRV:i+NumSamplesOverFirstHRV))/(SamplesforHRV-1);
            HRV(i+NumSamplesOverFirstHRV) = sqrt(sum((R_R_Data(NumSamplesOverFirstHRV+i+3-SamplesforHRV:i+NumSamplesOverFirstHRV)-HRVMean).^2)/(SamplesforHRV-2));
            
        end
    subplot(3,1,3);
    timeHRV = transpose(0:1/Fs_BR_RR:length(HRV)/Fs_BR_RR-1/Fs_BR_RR);
    plot(timeHRV(1:NumSamplesOverFirstHRV),HRV(1:NumSamplesOverFirstHRV),'r');hold on;
    plot(timeHRV(NumSamplesOverFirstHRV+1:length(timeHRV)),HRV(NumSamplesOverFirstHRV+1:length(HRV)),'b');
    ylabel('HRV Data');xlabel('Time (seconds)');
    legend('HRV Invalid Data','HRV Valid Data');
    grid on
    
    end
else
    % Need > 5min of data for HRV
    disp('Data Length too small for HRV');
end
    
end