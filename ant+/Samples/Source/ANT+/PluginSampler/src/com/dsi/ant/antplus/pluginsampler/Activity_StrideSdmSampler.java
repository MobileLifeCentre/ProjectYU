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
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.ICalorieDataReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.IComputationTimestampReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.IDataLatencyReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.IDistanceReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.IInstantaneousCadenceReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.IInstantaneousSpeedReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.ISensorStatusReceiver;
import com.dsi.ant.plugins.antplus.pcc.AntPlusStrideSdmPcc.IStrideCountReceiver;

import java.math.BigDecimal;

/**
 * Connects to Stride Sdm Plugin and receives data
 */
public class Activity_StrideSdmSampler extends Activity
{
    AntPlusStrideSdmPcc sdmPcc = null;
    
    TextView tv_status;
    
    TextView tv_msgsRcvdCount;
    
    TextView tv_instantaneousSpeed;
    TextView tv_instantaneousCadence;
    TextView tv_ComputationTimestamp;
    
    TextView tv_cumulativeDistance;
    TextView tv_cumulativeStrides;
    
    TextView tv_cumulativeCalories;
    
    TextView tv_updateLatency;
    
    TextView tv_StatusFlagLocation;
    TextView tv_StatusFlagBattery;
    TextView tv_StatusFlagHealth;
    TextView tv_StatusFlagUseState;
    
    TextView tv_manufacturerID;
    TextView tv_serialNumber;
    TextView tv_modelNumber;
    
    TextView tv_hardwareRevision;
    TextView tv_softwareRevision;

    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_stride_sdm);
        
        tv_status = (TextView)findViewById(R.id.textView_Status);
        
        tv_msgsRcvdCount = (TextView)findViewById(R.id.textView_MsgsRcvdCount);
        
        tv_instantaneousSpeed = (TextView)findViewById(R.id.textView_InstantaneousSpeed);
        tv_instantaneousCadence = (TextView)findViewById(R.id.textView_InstantaneousCadence);
        tv_ComputationTimestamp = (TextView)findViewById(R.id.textView_ComputationTimestamp);
        
        tv_cumulativeDistance = (TextView)findViewById(R.id.textView_CumulativeDistance);
        tv_cumulativeStrides = (TextView)findViewById(R.id.textView_CumulativeStrides);
        
        tv_cumulativeCalories = (TextView)findViewById(R.id.textView_CumulativeCalories);
        
        tv_updateLatency = (TextView)findViewById(R.id.textView_UpdateLatency);
        
        tv_StatusFlagLocation = (TextView)findViewById(R.id.textView_StatusFlagLocation);
        tv_StatusFlagBattery = (TextView)findViewById(R.id.textView_StatusFlagBattery);
        tv_StatusFlagHealth = (TextView)findViewById(R.id.textView_StatusFlagHealth);
        tv_StatusFlagUseState = (TextView)findViewById(R.id.textView_StatusFlagUseState);
        
        tv_manufacturerID = (TextView)findViewById(R.id.textView_ManufacturerID);
        tv_serialNumber = (TextView)findViewById(R.id.textView_SerialNumber);
        
        tv_modelNumber = (TextView)findViewById(R.id.textView_ModelNumber);
        
        tv_hardwareRevision = (TextView)findViewById(R.id.textView_HardwareRevision);
        tv_softwareRevision = (TextView)findViewById(R.id.textView_SoftwareRevision);
        
        resetPcc();
    }

    private void resetPcc()
    {
        if(sdmPcc != null)
        {
            sdmPcc.releaseAccess();
            sdmPcc = null;
        }
        
        tv_status.setText("Connecting...");
        
        tv_msgsRcvdCount.setText("---");
        
        tv_instantaneousSpeed.setText("---");
        tv_instantaneousCadence.setText("---");
        tv_ComputationTimestamp.setText("---");
        
        tv_cumulativeDistance.setText("---");
        tv_cumulativeStrides.setText("---");
        
        tv_cumulativeCalories.setText("---");
        
        tv_updateLatency.setText("---");
        
        tv_StatusFlagLocation.setText("---");
        tv_StatusFlagBattery.setText("---");
        tv_StatusFlagHealth.setText("---");
        tv_StatusFlagUseState.setText("---");
        
        tv_manufacturerID.setText("---");
        tv_serialNumber.setText("---");
        
        tv_modelNumber.setText("---");
        
        tv_hardwareRevision.setText("---");
        tv_softwareRevision.setText("---");
        
        AntPlusStrideSdmPcc.requestAccess(this, this,
                new IPluginAccessResultReceiver<AntPlusStrideSdmPcc>()
                    {                    
                        public void onResultReceived(AntPlusStrideSdmPcc result, int resultCode,
                                int initialDeviceStateCode)
                        {
                            switch(resultCode)
                            {
                                case AntPluginMsgDefines.MSG_REQACC_RESULT_whatSUCCESS:
                                    sdmPcc = result;
                                    tv_status.setText(result.getDeviceName() + ": " + AntPlusStrideSdmPcc.statusCodeToPrintableString(initialDeviceStateCode));
                                    subscribeToEvents();
                                    break;
                                case AntPluginMsgDefines.MSG_REQACC_RESULT_whatCHANNELNOTAVAILABLE:
                                    Toast.makeText(Activity_StrideSdmSampler.this, "Channel Not Available", Toast.LENGTH_SHORT).show();
                                    tv_status.setText("Error. Do Menu->Reset.");
                                    break;
                                case AntPluginMsgDefines.MSG_REQACC_RESULT_whatOTHERFAILURE:
                                    Toast.makeText(Activity_StrideSdmSampler.this, "RequestAccess failed. See logcat for details.", Toast.LENGTH_SHORT).show();
                                    tv_status.setText("Error. Do Menu->Reset.");
                                    break;
                                case AntPluginMsgDefines.MSG_REQACC_RESULT_whatDEPENDENCYNOTINSTALLED:
                                    tv_status.setText("Error. Do Menu->Reset.");
                                    AlertDialog.Builder adlgBldr = new AlertDialog.Builder(Activity_StrideSdmSampler.this);
                                    adlgBldr.setTitle("Missing Dependency");
                                    adlgBldr.setMessage("The required application\n\"" + AntPlusStrideSdmPcc.getMissingDependencyName() + "\"\n is not installed. Do you want to launch the Play Store to search for it?");
                                    adlgBldr.setCancelable(true);
                                    adlgBldr.setPositiveButton("Go to Store", new OnClickListener()
                                            {
                                                public void onClick(DialogInterface dialog, int which)
                                                {
                                                    Intent startStore = null;
                                                    startStore = new Intent(Intent.ACTION_VIEW,Uri.parse("market://details?id=" + AntPlusStrideSdmPcc.getMissingDependencyPackageName()));
                                                    startStore.addFlags(Intent.FLAG_ACTIVITY_NEW_TASK);
                                                    
                                                    Activity_StrideSdmSampler.this.startActivity(startStore);                                                
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
                                    Toast.makeText(Activity_StrideSdmSampler.this, "Unrecognized result: " + resultCode, Toast.LENGTH_SHORT).show();
                                    tv_status.setText("Error. Do Menu->Reset.");
                                    break;
                            } 
                        }
                        
                        private void subscribeToEvents()
                        {
                            sdmPcc.subscribeInstantaneousSpeedEvent(
                                new IInstantaneousSpeedReceiver()
                                {
                                    public void onNewInstantaneousSpeed(
                    				    final int currentMessageCount,
                    				    final BigDecimal instantaneousSpeed)
                    			    {
                        				runOnUiThread(
                    				        new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                	tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));  
                                                	
                                                	tv_instantaneousSpeed.setText(String.valueOf(instantaneousSpeed));
                                                }
                                            });
                    			    }
                                    	    
                            	});
                    	
                            sdmPcc.subscribeInstantaneousCadenceEvent(
                                new IInstantaneousCadenceReceiver()
                                {
                    			    public void onNewInstantaneousCadence(
                    				    final int currentMessageCount,
                    				    final BigDecimal instantaneousCadence)
                    			    {
                        				runOnUiThread(
                    				        new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                	tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));  
                                                	
                                                	tv_instantaneousCadence.setText(String.valueOf(instantaneousCadence));
                                                }
                                            });
                    			    }
                                });
                    	
                            sdmPcc.subscribeDistanceEvent(
                                new IDistanceReceiver()
                                {
                                    public void onNewDistance(final int currentMessageCount, final BigDecimal cumulativeDistance)
                                    {
                                        runOnUiThread(
                                            new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                    tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                    
                                                    tv_cumulativeDistance.setText(String.valueOf(cumulativeDistance));
                                                }
                                            });
                                    }
                                });
                            
                            sdmPcc.subscribeStrideCountEvent(
                                new IStrideCountReceiver()
                                {
                                    public void onNewStrideCount(final int currentMessageCount, final long cumulativeStrides)
                                    {
                                        runOnUiThread(
                                            new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                    tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                    
                                                    tv_cumulativeStrides.setText(String.valueOf(cumulativeStrides));
                                                }
                                            });
                                    }
                                });
                            
                            sdmPcc.subscribeComputationTimestampEvent(
                                new IComputationTimestampReceiver()
                                {
                                    public void onNewComputationTimestamp(final int currentMessageCount,
                                            final BigDecimal timestampOfLastComputation)
                                    {
                                        runOnUiThread(
                                            new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                    tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                    
                                                    tv_ComputationTimestamp.setText(String.valueOf(timestampOfLastComputation));
                                                }
                                            });
                                    }
                                });
                            
                            sdmPcc.subscribeDataLatencyEvent(
                                new IDataLatencyReceiver()
                                {
                                    public void onNewDataLatency(final int currentMessageCount, final BigDecimal updateLatency)
                                    {
                                        runOnUiThread(
                                            new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                    tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                    
                                                    tv_updateLatency.setText(String.valueOf(updateLatency));
                                                }
                                            });
                                    }
                                });
                            
                            sdmPcc.subscribeSensorStatusEvent(new ISensorStatusReceiver()
                                {
                                    public void onNewSensorStatus(final int currentMessageCount, final int SensorLocation, final int BatteryStatus,
                                            final int SensorHealth, final int UseState)
                                    {
                                        runOnUiThread(
                                            new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                    tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                    
                                                    tv_StatusFlagLocation.setText(String.valueOf(SensorLocation));
                                                    tv_StatusFlagBattery.setText(String.valueOf(BatteryStatus));
                                                    tv_StatusFlagHealth.setText(String.valueOf(SensorHealth));
                                                    tv_StatusFlagUseState.setText(String.valueOf(UseState));
                                                }
                                            });
                                    }
                                });
    
                    	
                            sdmPcc.subscribeCalorieDataEvent(
                                new ICalorieDataReceiver()
                                {
                    			    public void onNewCalorieData(
                    				    final int currentMessageCount,
                    				    final long cumulativeCalories)
                    			    {
                        				runOnUiThread(
                    				        new Runnable()
                                            {                                            
                                                public void run()
                                                {
                                                	tv_msgsRcvdCount.setText(String.valueOf(currentMessageCount));
                                                	
                                                	tv_cumulativeCalories.setText(String.valueOf(cumulativeCalories));
                                                }
                                            });
                    			    }
                			    });
                    	
                            sdmPcc.subscribeManufacturerIdentificationEvent(
                    	        new IManufacturerIdentificationReceiver()
                                {                            
                                    public void onNewManufacturerIdentification(final int currentMessageCount, final int hardwareRevision,
                                            final int manufacturerID, final int modelNumber)
                                    {
                                        runOnUiThread(
                                            new Runnable()
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
                    
                            sdmPcc.subscribeProductInformationEvent(
                                new IProductInformationReceiver()
                                {                                    
                                    public void onNewProductInformation(final int currentMessageCount, final int softwareRevision,
                                            final long serialNumber)
                                    {
                                        runOnUiThread(
                                            new Runnable()
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
                new IDeviceStateChangeReceiver()
                    {                    
                        public void onDeviceStateChange(final int newDeviceState)
                        {
                            runOnUiThread(
                                new Runnable()
                                {                                            
                                    public void run()
                                    {
                                        tv_status.setText(sdmPcc.getDeviceName() + ": " + AntPlusStrideSdmPcc.statusCodeToPrintableString(newDeviceState));
                                    }
                                });
                        }
                    } 
                );
    }

    @Override
    protected void onDestroy()
    {
        if(sdmPcc != null)
        {
            sdmPcc.releaseAccess();
            sdmPcc = null;
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
