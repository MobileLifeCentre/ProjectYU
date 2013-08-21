/*
This software is subject to the license described in the License.txt file 
included with this software distribution. You may not use this file except in compliance 
with this license.

Copyright (c) Dynastream Innovations Inc. 2013
All rights reserved.
*/

package com.dsi.ant.antplus.pluginsampler;

import android.app.Activity;
import android.app.AlertDialog;
import android.content.DialogInterface;
import android.content.DialogInterface.OnClickListener;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.TextView;
import android.widget.Toast;

import com.dsi.ant.plugins.AntPluginMsgDefines;
import com.dsi.ant.plugins.AntPluginPcc.IDeviceStateChangeReceiver;
import com.dsi.ant.plugins.AntPluginPcc.IPluginAccessResultReceiver;
import com.dsi.ant.plugins.antplus.AntPlusCommonPcc.IManufacturerIdentificationReceiver;
import com.dsi.ant.plugins.antplus.AntPlusCommonPcc.IProductInformationReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusEnvironmentPcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusEnvironmentPcc.ITemperatureDataReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusHeartRatePcc;

import java.math.BigDecimal;

/**
 * Connects to Environment Plugin and display all the event data.
 */
public class Activity_EnvironmentSampler extends Activity
{
    AntPlusEnvironmentPcc envPcc = null;
    
    TextView tv_status;
    
    TextView tv_msgsRcvdCount;
    
    TextView tv_currentTemperature;
    TextView tv_eventCount;
    TextView tv_lowLast24Hours;    
    TextView tv_highLast24Hours;

    TextView tv_hardwareRevision;
    TextView tv_manufacturerID;
    TextView tv_modelNumber;
    
    TextView tv_softwareRevision;
    TextView tv_serialNumber;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_environment);
        
        tv_status = (TextView)findViewById(R.id.textView_Status);
        
        tv_msgsRcvdCount = (TextView)findViewById(R.id.textView_MsgsRcvdCount);
        
        tv_currentTemperature = (TextView)findViewById(R.id.textView_CurrentTemperature);
        tv_eventCount = (TextView)findViewById(R.id.textView_EventCount);
        tv_lowLast24Hours = (TextView)findViewById(R.id.textView_LowLast24Hours);   
        tv_highLast24Hours = (TextView)findViewById(R.id.textView_HighLast24Hours);

        tv_hardwareRevision = (TextView)findViewById(R.id.textView_HardwareRevision);
        tv_manufacturerID = (TextView)findViewById(R.id.textView_ManufacturerID);
        tv_modelNumber = (TextView)findViewById(R.id.textView_ModelNumber);
        
        tv_softwareRevision = (TextView)findViewById(R.id.textView_SoftwareRevision);
        tv_serialNumber = (TextView)findViewById(R.id.textView_SerialNumber);
        
        resetPcc();
    }

    /**
     * Resets the PCC connection to request access again and clears any existing display data.
     */ 
    private void resetPcc()
    {
        //Release the old access if it exists
        if(envPcc != null)
        {
            envPcc.releaseAccess();
            envPcc = null;
        }
        
        
        //Reset the text display
        tv_status.setText("Connecting...");
        
        tv_msgsRcvdCount.setText("---");
        
        tv_currentTemperature.setText("---");
        tv_eventCount.setText("---");
        tv_lowLast24Hours.setText("---");    
        tv_highLast24Hours.setText("---");

        tv_hardwareRevision.setText("---");
        tv_manufacturerID.setText("---");
        tv_modelNumber.setText("---");
        
        tv_softwareRevision.setText("---");
        tv_serialNumber.setText("---");
        
        
        //Make the access request
        AntPlusEnvironmentPcc.requestAccess(this, this,
                new IPluginAccessResultReceiver<AntPlusEnvironmentPcc>()
                {         
                    //Handle the result, connecting to events on success or reporting failure to user.
                    
                    public void onResultReceived(AntPlusEnvironmentPcc result, int resultCode,
                            int initialDeviceStateCode)
                    {
                        switch(resultCode)
                        {
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatSUCCESS:
                                envPcc = result;
                                tv_status.setText(result.getDeviceName() + ": " + AntPlusEnvironmentPcc.statusCodeToPrintableString(initialDeviceStateCode));
                                subscribeToEvents();
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatCHANNELNOTAVAILABLE:
                                Toast.makeText(Activity_EnvironmentSampler.this, "Channel Not Available", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatOTHERFAILURE:
                                Toast.makeText(Activity_EnvironmentSampler.this, "RequestAccess failed. See logcat for details.", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatDEPENDENCYNOTINSTALLED:
                                tv_status.setText("Error. Do Menu->Reset.");
                                AlertDialog.Builder adlgBldr = new AlertDialog.Builder(Activity_EnvironmentSampler.this);
                                adlgBldr.setTitle("Missing Dependency");
                                adlgBldr.setMessage("The required application\n\"" + AntPlusEnvironmentPcc.getMissingDependencyName() + "\"\n is not installed. Do you want to launch the Play Store to search for it?");
                                adlgBldr.setCancelable(true);
                                adlgBldr.setPositiveButton("Go to Store", new OnClickListener()
                                        {
                                            
                                            public void onClick(DialogInterface dialog, int which)
                                            {
                                                Intent startStore = null;
                                                startStore = new Intent(Intent.ACTION_VIEW,Uri.parse("market://details?id=" + AntPlusEnvironmentPcc.getMissingDependencyPackageName()));
                                                startStore.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                                                
                                                Activity_EnvironmentSampler.this.startActivity(startStore);                                                
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
                                Toast.makeText(Activity_EnvironmentSampler.this, "Unrecognized result: " + resultCode, Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                        } 
                    }
                    
                    /**
                     * Subscribe to all the heart rate events, connecting them to display their data.
                     */
                    private void subscribeToEvents()
                    {
                        envPcc.subscribeTemperatureDataEvent(new ITemperatureDataReceiver()
                                {
                                    
                                    
                                    public void onNewTemperatureData(final int currentMessageCount, final BigDecimal currentTemperature,
                                            final long eventCount, final BigDecimal lowLast24Hours, final BigDecimal highLast24Hours)
                                    {
                                        runOnUiThread(new Runnable()
                                        {                                            
                                            
                                            public void run()
                                            {
                                                tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                
                                                tv_currentTemperature.setText(String.valueOf(currentTemperature));
                                                tv_eventCount.setText(String.valueOf(eventCount));
                                                tv_lowLast24Hours.setText(String.valueOf(lowLast24Hours));
                                                tv_highLast24Hours.setText(String.valueOf(highLast24Hours));
                                            }
                                        });                                
                                    }
                                });
                        
                        envPcc.subscribeManufacturerIdentificationEvent(new IManufacturerIdentificationReceiver()
                                {
                                    
                                    
                                    public void onNewManufacturerIdentification(final int currentMessageCount, final int hardwareRevision,
                                            final int manufacturerID, final int modelNumber)
                                    {
                                        runOnUiThread(new Runnable()
                                        {                                            
                                            
                                            public void run()
                                            {
                                                tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                
                                                tv_hardwareRevision.setText(String.valueOf(hardwareRevision));
                                                tv_manufacturerID.setText(String.valueOf(manufacturerID));
                                                tv_modelNumber.setText(String.valueOf(modelNumber));
                                            }
                                        });
                                    }
                                });
                        
                        envPcc.subscribeProductInformationEvent(new IProductInformationReceiver()
                                {
                                    
                                    
                                    public void onNewProductInformation(final int currentMessageCount, final int softwareRevision,
                                            final long serialNumber)
                                    {
                                        runOnUiThread(new Runnable()
                                        {                                            
                                            
                                            public void run()
                                            {
                                                tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                
                                                tv_softwareRevision.setText(String.valueOf(softwareRevision));
                                                tv_serialNumber.setText(String.valueOf(serialNumber));
                                            }
                                        });
                                    }
                                });
                    }
                }, 
                //Receives state changes and shows it on the status display line
                new IDeviceStateChangeReceiver()
                        {                    
                            
                            public void onDeviceStateChange(final int newDeviceState)
                            {
                                runOnUiThread(new Runnable()
                                        {                                            
                                            
                                            public void run()
                                            {
                                                tv_status.setText(envPcc.getDeviceName() + ": " + AntPlusHeartRatePcc.statusCodeToPrintableString(newDeviceState));
                                                if(newDeviceState == AntPluginMsgDefines.DeviceStateCodes.DEAD)
                                                    envPcc = null;
                                            }
                                        });
                                
                                
                            }
                        } );
    }

    @Override
    protected void onDestroy()
    {
        if(envPcc != null)
        {
            envPcc.releaseAccess();
            envPcc = null;
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
