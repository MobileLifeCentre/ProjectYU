/*
This software is subject to the license described in the License.txt file 
included with this software distribution. You may not use this file except in compliance 
with this license.

Copyright (c) Dynastream Innovations Inc. 2013
All rights reserved.
*/

package com.dsi.ant.antplus.pluginsampler;

import com.dsi.ant.plugins.AntPluginMsgDefines;
import com.dsi.ant.plugins.AntPluginPcc.IDeviceStateChangeReceiver;
import com.dsi.ant.plugins.AntPluginPcc.IPluginAccessResultReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusControlsPcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusControlsPcc.IGenericCommandReceiver;
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
 * Connects to Controls Plugin, using the Generic mode, and receives generic commands from a remote
 */
public class Activity_GenericControllableDeviceSampler extends Activity 
{
	AntPlusControlsPcc ctrlPcc = null;
	
	int DeviceID = 0; // Set to Zero for the pluging to automatically generate the ID
	
	TextView tv_status;
	TextView tv_deviceID;
	
    TextView tv_msgsRcvdCount;
    
    TextView tv_commandNumber;
    TextView tv_sequenceNumber;
    TextView tv_remoteSerialNumber;
    TextView tv_remoteManufacturerID;
    TextView tv_commandData;
    

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

        
        AntPlusControlsPcc.requestAccessGenericMode(this, new IPluginAccessResultReceiver<AntPlusControlsPcc>()
                {                    
                    public void onResultReceived(AntPlusControlsPcc result, int resultCode, int initialDeviceStateCode)
                    {
                        switch(resultCode)
                        {
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatSUCCESS:
                                ctrlPcc = result;
                                tv_status.setText(result.getDeviceName() + ": " + AntPlusControlsPcc.statusCodeToPrintableString(initialDeviceStateCode));
                                tv_deviceID.setText(String.valueOf(ctrlPcc.getAntDeviceID()));
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatCHANNELNOTAVAILABLE:
                                Toast.makeText(Activity_GenericControllableDeviceSampler.this, "Channel Not Available", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatOTHERFAILURE:
                                Toast.makeText(Activity_GenericControllableDeviceSampler.this, "RequestAccess failed. See logcat for details.", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatDEPENDENCYNOTINSTALLED:
                                tv_status.setText("Error. Do Menu->Reset.");
                                AlertDialog.Builder adlgBldr = new AlertDialog.Builder(Activity_GenericControllableDeviceSampler.this);
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
                                                
                                                Activity_GenericControllableDeviceSampler.this.startActivity(startStore);                                                
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
                                Toast.makeText(Activity_GenericControllableDeviceSampler.this, "Unrecognized result: " + resultCode, Toast.LENGTH_SHORT).show();
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
                new IGenericCommandReceiver()
                        {

							
							public int onNewGenericCommand(final int currentMessageCount, final int serialNumber, 
									final int manufacturerID, final int sequenceNumber, final int commandNumber) 
							{
								runOnUiThread(new Runnable()
                                {                                            
                                    
                                    public void run()
                                    {
                                        tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                        tv_sequenceNumber.setText(String.valueOf(sequenceNumber));
                                        tv_remoteSerialNumber.setText(String.valueOf(serialNumber));
                                        tv_remoteManufacturerID.setText(String.valueOf(manufacturerID));
                                        
                                        switch(commandNumber)
                                        {
                                        	case AntPlusControlsPcc.GenericCommandNumber.MENU_UP:
                                        		tv_commandNumber.setText("MENU UP"); 
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.MENU_DOWN:
                                        		tv_commandNumber.setText("MENU DOWN");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.MENU_SELECT:
                                        		tv_commandNumber.setText("MENU SELECT");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.MENU_BACK:
                                        		tv_commandNumber.setText("MENU BACK");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.HOME:
                                        		tv_commandNumber.setText("HOME");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.START:
                                        		tv_commandNumber.setText("START");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.STOP:
                                        		tv_commandNumber.setText("STOP");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.RESET:
                                        		tv_commandNumber.setText("RESET");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.LENGTH:
                                        		tv_commandNumber.setText("LENGTH");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.LAP:
                                        		tv_commandNumber.setText("LAP");
                                        		break;
                                        	case AntPlusControlsPcc.GenericCommandNumber.NO_COMMAND:
                                        		tv_commandNumber.setText("NO COMMAND");
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
                DeviceID);
    }
    
    @Override
    protected void onDestroy()
    {	
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
}
