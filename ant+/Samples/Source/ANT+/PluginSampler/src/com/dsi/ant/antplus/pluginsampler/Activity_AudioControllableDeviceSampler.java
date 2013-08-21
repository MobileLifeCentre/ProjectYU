/*
This software is subject to the license described in the License.txt file 
included with this software distribution. You may not use this file except in compliance 
with this license.

Copyright (c) Dynastream Innovations Inc. 2013
All rights reserved.
*/

package com.dsi.ant.antplus.pluginsampler;

import java.util.Timer;
import java.util.TimerTask;

import com.dsi.ant.plugins.AntPluginMsgDefines;
import com.dsi.ant.plugins.AntPluginPcc.IDeviceStateChangeReceiver;
import com.dsi.ant.plugins.AntPluginPcc.IPluginAccessResultReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusControlsPcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusControlsPcc.AudioDeviceCapabilities;
import com.dsi.ant.plugins.antplus.pcc.AntPlusControlsPcc.IAudioCommandReceiver;
import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.Intent;
import android.content.DialogInterface.OnClickListener;
import android.net.Uri;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.TextView;
import android.widget.Toast;

/**
 * Connects to Controls Plugin, using the Audio mode, and receives audio commands from a remote
 */
public class Activity_AudioControllableDeviceSampler extends Activity 
{
	AntPlusControlsPcc ctrlPcc = null;
	AudioDeviceCapabilities capabilities = new AudioDeviceCapabilities();
	
	int DeviceID = 0; // Set to Zero for the pluging to automatically generate the ID
	
	TextView tv_status;
	TextView tv_deviceID;
	
    TextView tv_msgsRcvdCount;
    
    TextView tv_commandNumber;
    TextView tv_sequenceNumber;
    TextView tv_remoteSerialNumber;
    TextView tv_remoteManufacturerID;
    TextView tv_commandData;
    
    private Timer updateTimer;
    
    
    
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_controls);
        
        tv_status = (TextView)findViewById(R.id.textView_Status);
        tv_deviceID = (TextView)findViewById(R.id.textView_DeviceID);
        
        tv_msgsRcvdCount = (TextView)findViewById(R.id.textView_MsgsRcvdCount);
        
        tv_commandNumber = (TextView)findViewById(R.id.textView_CommandNumber);
        tv_sequenceNumber = (TextView)findViewById(R.id.textView_SequenceNumber);
        tv_remoteSerialNumber = (TextView)findViewById(R.id.textView_RemoteSerialNumber);        
        tv_remoteManufacturerID = (TextView)findViewById(R.id.textView_RemoteManufacturerID);
        tv_commandData = (TextView)findViewById(R.id.textView_CommandData);
        
        resetPcc();
        
        updateTimer = new Timer();  // Timer is used for very rudimentary data simulation
        
        // Configure capabilities
        capabilities.customRepeatModeSupport = true;
        capabilities.customShuffleModeSupport = false;
    }
    
    private void resetPcc()
    {
        if(ctrlPcc != null)
        {
            ctrlPcc.releaseAccess();
            ctrlPcc = null;
        }
        
        tv_status.setText("Connecting...");
        tv_deviceID.setText("---");
        
        tv_msgsRcvdCount.setText("---");
        
        tv_commandNumber.setText("---");
        tv_sequenceNumber.setText("---");
        tv_remoteSerialNumber.setText("---");        
        tv_remoteManufacturerID.setText("---");
        tv_commandData.setText("---");

        
        AntPlusControlsPcc.requestAccessAudioMode(this, new IPluginAccessResultReceiver<AntPlusControlsPcc>()
                {                    
                    public void onResultReceived(AntPlusControlsPcc result, int resultCode, int initialDeviceStateCode)
                    {
                        switch(resultCode)
                        {
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatSUCCESS:
                                ctrlPcc = result;
                                tv_status.setText(result.getDeviceName() + ": " + AntPlusControlsPcc.statusCodeToPrintableString(initialDeviceStateCode));
                                tv_deviceID.setText(String.valueOf(ctrlPcc.getAntDeviceID()));
                                updateTimer.schedule(new SimulateDataTask(), 2000, 2000); // Start generating simulated data
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatCHANNELNOTAVAILABLE:
                                Toast.makeText(Activity_AudioControllableDeviceSampler.this, "Channel Not Available", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatOTHERFAILURE:
                                Toast.makeText(Activity_AudioControllableDeviceSampler.this, "RequestAccess failed. See logcat for details.", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatDEPENDENCYNOTINSTALLED:
                                tv_status.setText("Error. Do Menu->Reset.");
                                AlertDialog.Builder adlgBldr = new AlertDialog.Builder(Activity_AudioControllableDeviceSampler.this);
                                adlgBldr.setTitle("Missing Dependency");
                                adlgBldr.setMessage("The required application\n\"" + AntPlusControlsPcc.getMissingDependencyName() + "\"\n is not installed. Do you want to launch the Play Store to search for it?");
                                adlgBldr.setCancelable(true);
                                adlgBldr.setPositiveButton("Go to Store", new OnClickListener()
                                        {
                                            public void onClick(DialogInterface dialog, int which)
                                            {
                                                Intent startStore = null;
                                                startStore = new Intent(Intent.ACTION_VIEW,Uri.parse("market://details?id=" + AntPlusControlsPcc.getMissingDependencyPackageName()));
                                                startStore.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                                                
                                                Activity_AudioControllableDeviceSampler.this.startActivity(startStore);                                                
                                            }
                                        });
                                adlgBldr.setNegativeButton("Cancel", new OnClickListener()
                                        {
                                            public void onClick(DialogInterface dialog, int which)
                                            {
                                                dialog.dismiss();
                                            }
                                        });
                                
                                final AlertDialog waitDialog = adlgBldr.create();
                                waitDialog.show();
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatUSERCANCELLED:
                                tv_status.setText("Cancelled. Do Menu->Reset.");
                                break;
                            default:
                                Toast.makeText(Activity_AudioControllableDeviceSampler.this, "Unrecognized result: " + resultCode, Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                        } 
                    }                    
                }, 
                new IDeviceStateChangeReceiver()
                        {                    
                            public void onDeviceStateChange(final int newDeviceState)
                            {
                                runOnUiThread(new Runnable()
                                        {                                            
                                            public void run()
                                            {
                                                tv_status.setText(ctrlPcc.getDeviceName() + ": " + AntPlusControlsPcc.statusCodeToPrintableString(newDeviceState));
                                            }
                                        });
                                
                                
                            }
                        },
                new IAudioCommandReceiver()
                        {

							public int onNewAudioCommand(final int currentMessageCount, final int serialNumber,
									final int sequenceNumber, final int commandNumber, final int commandData) 
							{
								runOnUiThread(new Runnable()
                                {                                            
                                    public void run()
                                    {
                                        tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                        tv_sequenceNumber.setText(String.valueOf(sequenceNumber));
                                        tv_remoteSerialNumber.setText(String.valueOf(serialNumber));
                                        tv_commandData.setText(String.valueOf(commandData));
                                        
                                        switch(commandNumber)
                                        {
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.PLAY:
                                        		tv_commandNumber.setText("PLAY"); 
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.PAUSE:
                                        		tv_commandNumber.setText("PAUSE");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.STOP:
                                        		tv_commandNumber.setText("STOP");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.VOLUME_UP:
                                        		tv_commandNumber.setText("VOL UP");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.VOLUME_DOWN:
                                        		tv_commandNumber.setText("VOL DOWN");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.MUTE_UNMUTE:
                                        		tv_commandNumber.setText("MUTE/UNMUTE");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.AHEAD:
                                        		tv_commandNumber.setText("TRACK AHEAD");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.BACK:
                                        		tv_commandNumber.setText("TRACK BACK");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.REPEAT_CURRENT_TRACK:
                                        		tv_commandNumber.setText("REPEAT TRACK");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.REPEAT_ALL:
                                        		tv_commandNumber.setText("REPEAT ALL");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.REPEAT_OFF:
                                        		tv_commandNumber.setText("REPEAT OFF");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.SHUFFLE_TRACKS:
                                        		tv_commandNumber.setText("SHUFFLE TRACKS");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.SHUFFLE_ALBUMS:
                                        		tv_commandNumber.setText("SHUFFLE ALBUMS");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.SHUFFLE_OFF:
                                        		tv_commandNumber.setText("SHUFFLE OFF");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.FAST_FORWARD:
                                        		tv_commandNumber.setText("FAST FWD");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.FAST_REWIND:
                                        		tv_commandNumber.setText("FAST RWND");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.CUSTOM_REPEAT:
                                        		tv_commandNumber.setText("CUSTOM REPEAT");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.CUSTOM_SHUFFLE:
                                        		tv_commandNumber.setText("CUSTOM SHUFFLE");
                                        		break;
                                        	case AntPlusControlsPcc.AudioVideoCommandNumber.RECORD:
                                        		tv_commandNumber.setText("RECORD");
                                        		break;
                                    		default:
                                    			tv_commandNumber.setText(String.valueOf(commandNumber)); 
                                    			break;
                                        }
                                    }
                                });
								return AntPlusControlsPcc.CommandStatus.PASS;
							}
                        	
                        },
                capabilities, DeviceID);
    }
    
    @Override
    protected void onDestroy()
    {
    	if(updateTimer != null)
    	{
    		updateTimer.cancel();
    		updateTimer = null;
    	}    	
        if(ctrlPcc != null)
        {
            ctrlPcc.releaseAccess();
            ctrlPcc = null;
        }
        super.onDestroy();
    }
    
    @Override
    public boolean onCreateOptionsMenu(Menu menu)
    {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate(R.menu.activity_heart_rate, menu);
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item)
    {
        switch(item.getItemId())
        {
            case R.id.menu_reset:
                resetPcc();
                tv_status.setText("Resetting...");
                return true;
            default:
                return super.onOptionsItemSelected(item);                
        }
    }
    
 // Very rudimentary simulation just for testing, cycle through values every 2 seconds
    class SimulateDataTask extends TimerTask
    {
    	int totalTrackTime = 248; // 4:08 min
    	int currentTime = 0;
    	int currentVolume = 0;
    	int deviceState = AntPlusControlsPcc.AudioDeviceState.OFF;
    	int repeatState = AntPlusControlsPcc.AudioRepeatState.OFF_UNSUPPORTED;
    	int shuffleState = AntPlusControlsPcc.AudioShuffleState.OFF_UNSUPPORTED;   	

    	@Override
    	public void run()
    	{
    		// For testing purposes, this just generates random values for volume & track time
    		if(ctrlPcc != null)
    		{
    			ctrlPcc.updateAudioStatus(currentVolume, totalTrackTime, currentTime, deviceState, repeatState, shuffleState);
    			
    			// Cycle current track time from 0 to total track time
    			if(currentTime == totalTrackTime)
    				currentTime = 0;
    			else
    				currentTime += 2;
    			
    			// Cycle volume from 0 - 100
    			if(currentVolume == 100)
    				currentVolume = 0;
    			else
    				currentVolume++;
    			
    			// Cycle through the states
    			if(deviceState == AntPlusControlsPcc.AudioDeviceState.REWIND)
    				deviceState = AntPlusControlsPcc.AudioDeviceState.OFF;
    			else
    				deviceState++;
    			
    			if(repeatState == AntPlusControlsPcc.AudioRepeatState.CUSTOM)
    				repeatState = AntPlusControlsPcc.AudioRepeatState.OFF_UNSUPPORTED;
    			else
    				repeatState++;
    			
    			if(shuffleState == AntPlusControlsPcc.AudioShuffleState.CUSTOM)
    				shuffleState = AntPlusControlsPcc.AudioShuffleState.OFF_UNSUPPORTED;  
    			else
    				shuffleState++;
    		}
    	}
    }

}
