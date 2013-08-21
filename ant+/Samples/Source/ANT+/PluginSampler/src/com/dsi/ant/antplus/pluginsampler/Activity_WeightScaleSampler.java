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
import android.view.View;
import android.widget.Button;
import android.widget.TextView;
import android.widget.Toast;

import com.dsi.ant.plugins.AntPluginMsgDefines;
import com.dsi.ant.plugins.AntPluginPcc.IDeviceStateChangeReceiver;
import com.dsi.ant.plugins.AntPluginPcc.IPluginAccessResultReceiver;
import com.dsi.ant.plugins.RequestStatusCode;
import com.dsi.ant.plugins.antplus.AntPlusCommonPcc.IManufacturerIdentificationReceiver;
import com.dsi.ant.plugins.antplus.AntPlusCommonPcc.IProductInformationReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusEnvironmentPcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusHeartRatePcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusWeightScalePcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusWeightScalePcc.IBasicMeasurementFinishedReceiver;

import java.math.BigDecimal;

/**
 * Connects to Environment Plugin and display all the event data.
 */
public class Activity_WeightScaleSampler extends Activity
{
    AntPlusWeightScalePcc wgtPcc = null;
    
    Button button_requestBasicMeasurement;
    
    TextView tv_status;
    
    TextView tv_msgsRcvdCount;
    
    TextView tv_bodyWeight;

    TextView tv_hardwareRevision;
    TextView tv_manufacturerID;
    TextView tv_modelNumber;
    
    TextView tv_softwareRevision;
    TextView tv_serialNumber;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_weightscale);
        
        button_requestBasicMeasurement = (Button)findViewById(R.id.button_requestBasicMeasurement);
        
        tv_status = (TextView)findViewById(R.id.textView_Status);
        
        tv_msgsRcvdCount = (TextView)findViewById(R.id.textView_MsgsRcvdCount);
        
        tv_bodyWeight = (TextView)findViewById(R.id.textView_BodyWeight);

        tv_hardwareRevision = (TextView)findViewById(R.id.textView_HardwareRevision);
        tv_manufacturerID = (TextView)findViewById(R.id.textView_ManufacturerID);
        tv_modelNumber = (TextView)findViewById(R.id.textView_ModelNumber);
        
        tv_softwareRevision = (TextView)findViewById(R.id.textView_SoftwareRevision);
        tv_serialNumber = (TextView)findViewById(R.id.textView_SerialNumber);
        
        button_requestBasicMeasurement.setOnClickListener(new View.OnClickListener()
                {                    
                    public void onClick(View v)
                    {
                        
                        boolean submitted = wgtPcc.requestBasicMeasurement(new IBasicMeasurementFinishedReceiver()
                                {
                                    
                                    public void onBasicMeasurementFinished(int currentMessageCount, final int statusCode,
                                            final BigDecimal bodyWeight)
                                    {
                                        runOnUiThread(new Runnable()
                                        {                                            
                                            
                                            public void run()
                                            { 
                                                button_requestBasicMeasurement.setEnabled(true);
                                                switch(statusCode)
                                                {
                                                    case RequestStatusCode.SUCCESS:
                                                        if(bodyWeight.intValue() == -1)
                                                            tv_bodyWeight.setText("Invalid");
                                                        else
                                                            tv_bodyWeight.setText(String.valueOf(bodyWeight) + "Kg");
                                                        break;
                                                    case RequestStatusCode.FAIL_ALREADY_BUSY_EXTERNAL:
                                                        tv_bodyWeight.setText("Fail: Busy");
                                                        break;
                                                    case RequestStatusCode.FAIL_DEVICE_COMMUNICATION_FAILURE:
                                                        tv_bodyWeight.setText("Fail: Comm Err");
                                                        break;
                                                }
                                            }
                                        });
                                    }
                                });
                        
                        if(submitted)
                        {
                            button_requestBasicMeasurement.setEnabled(false);
                            tv_bodyWeight.setText("Computing");
                        }
                    }
                });
        
        resetPcc();
    }

    /**
     * Resets the PCC connection to request access again and clears any existing display data.
     */ 
    private void resetPcc()
    {
        //Release the old access if it exists
        if(wgtPcc != null)
        {
            wgtPcc.releaseAccess();
            wgtPcc = null;
        }
        
        
        //Reset the text display
        tv_status.setText("Connecting...");
        
        button_requestBasicMeasurement.setEnabled(false);
        
        tv_msgsRcvdCount.setText("---");
        
        tv_bodyWeight.setText("---");

        tv_hardwareRevision.setText("---");
        tv_manufacturerID.setText("---");
        tv_modelNumber.setText("---");
        
        tv_softwareRevision.setText("---");
        tv_serialNumber.setText("---");
        
        
        //Make the access request
        AntPlusWeightScalePcc.requestAccess(this, this,
                new IPluginAccessResultReceiver<AntPlusWeightScalePcc>()
                {         
                    //Handle the result, connecting to events on success or reporting failure to user.
                    
                    public void onResultReceived(AntPlusWeightScalePcc result, int resultCode,
                            int initialDeviceStateCode)
                    {
                        switch(resultCode)
                        {
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatSUCCESS:
                                wgtPcc = result;
                                tv_status.setText(result.getDeviceName() + ": " + AntPlusWeightScalePcc.statusCodeToPrintableString(initialDeviceStateCode));
                                subscribeToEvents();
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatCHANNELNOTAVAILABLE:
                                Toast.makeText(Activity_WeightScaleSampler.this, "Channel Not Available", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatOTHERFAILURE:
                                Toast.makeText(Activity_WeightScaleSampler.this, "RequestAccess failed. See logcat for details.", Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                            case AntPluginMsgDefines.MSG_REQACC_RESULT_whatDEPENDENCYNOTINSTALLED:
                                tv_status.setText("Error. Do Menu->Reset.");
                                AlertDialog.Builder adlgBldr = new AlertDialog.Builder(Activity_WeightScaleSampler.this);
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
                                                
                                                Activity_WeightScaleSampler.this.startActivity(startStore);                                                
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
                                Toast.makeText(Activity_WeightScaleSampler.this, "Unrecognized result: " + resultCode, Toast.LENGTH_SHORT).show();
                                tv_status.setText("Error. Do Menu->Reset.");
                                break;
                        } 
                    }
                    
                    /**
                     * Subscribe to all the heart rate events, connecting them to display their data.
                     */
                    private void subscribeToEvents()
                    {
                        button_requestBasicMeasurement.setEnabled(true);
                        
                        wgtPcc.subscribeManufacturerIdentificationEvent(new IManufacturerIdentificationReceiver()
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
                        
                        wgtPcc.subscribeProductInformationEvent(new IProductInformationReceiver()
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
                                                tv_status.setText(wgtPcc.getDeviceName() + ": " + AntPlusHeartRatePcc.statusCodeToPrintableString(newDeviceState));
                                                if(newDeviceState == AntPluginMsgDefines.DeviceStateCodes.DEAD)
                                                    wgtPcc = null;
                                            }
                                        });
                                
                                
                            }
                        } );
    }

    @Override
    protected void onDestroy()
    {
        if(wgtPcc != null)
        {
            wgtPcc.releaseAccess();
            wgtPcc = null;
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
