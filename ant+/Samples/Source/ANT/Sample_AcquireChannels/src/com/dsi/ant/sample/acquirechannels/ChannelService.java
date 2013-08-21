/*
 * Copyright 2012 Dynastream Innovations Inc.
 *
 * Licensed under the Apache License, Version 2.0 (the "License"); you may not
 * use this file except in compliance with the License. You may obtain a copy of
 * the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS, WITHOUT
 * WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the
 * License for the specific language governing permissions and limitations under
 * the License.
 */
package com.dsi.ant.sample.acquirechannels;

import com.dsi.ant.sample.acquirechannels.ChannelController.ChannelBroadcastListener;

import com.dsi.ant.AntService;
import com.dsi.ant.channel.AntChannel;
import com.dsi.ant.channel.AntChannelProvider;
import com.dsi.ant.channel.ChannelNotAvailableException;
import com.dsi.ant.channel.PredefinedNetwork;

import android.app.Service;
import android.content.BroadcastReceiver;
import android.content.ComponentName;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.content.ServiceConnection;
import android.os.Binder;
import android.os.IBinder;
import android.os.RemoteException;
import android.util.Log;
import android.util.SparseArray;

import java.util.ArrayList;

public class ChannelService extends Service
{
    private static final String TAG = "ChannelService";
    
    private Object mCreateChannel_LOCK = new Object();
    
    SparseArray<ChannelController> mChannelControllerList = new SparseArray<ChannelController>();
    
    ChannelChangedListener mListener;
    
    int channelDeviceIdCounter = 0;
    
    private boolean mAntRadioServiceBound;
    private AntService mAntRadioService = null;
    private AntChannelProvider mAntChannelProvider = null;
    private boolean mChannelAvailable = false;
    
    private ServiceConnection mAntRadioServiceConnection = new ServiceConnection()
    {
        @Override
        public void onServiceConnected(ComponentName name, IBinder service)
        {
            mAntRadioService = new AntService(service);
            
            try {
                mAntChannelProvider = mAntRadioService.getChannelProvider();
                mChannelAvailable = (mAntChannelProvider.getNumChannelsAvailable() > 0);
                
                if(mChannelAvailable) {
                    if(null != mListener) {
                    mListener.onChannelAvailable(true);
                    }
                }
                
            } catch (RemoteException e) {
                // TODO Auto-generated catch block
                e.printStackTrace();
            }
        }
        
        @Override
        public void onServiceDisconnected(ComponentName name)
        {
            die("Binder Died");
            
            mAntChannelProvider = null;
            mAntRadioService = null;
            
            if(mChannelAvailable) { mListener.onChannelAvailable(false); }
            mChannelAvailable = false;
        }
        
    };
    
    public interface ChannelChangedListener
    {
        void onChannelChanged(ChannelInfo newInfo);
        void onChannelAvailable(boolean hasChannel); 
    }
    
    public class ChannelServiceComm extends Binder
    {
        void setOnChannelChangedListener(ChannelChangedListener listener)
        {
            mListener = listener;
        }
        
        ArrayList<ChannelInfo> getCurrentChannelInfoForAllChannels()
        {
            ArrayList<ChannelInfo> retList = new ArrayList<ChannelInfo>();
            for(int i = 0; i <  mChannelControllerList.size(); i++)
            {
                ChannelController channel = mChannelControllerList.valueAt(i);
                
                retList.add(channel.getCurrentInfo());
            }
            
            return retList;
        }
        
        ChannelInfo addNewChannel(final boolean isMaster) throws ChannelNotAvailableException
        {
            return createNewChannel(isMaster);
        }
        
        void clearAllChannels() { closeAllChannels(); }
        
        boolean isChannelAvailable() { return mChannelAvailable; }
    }
    
    private void closeAllChannels()
    {
        synchronized (mChannelControllerList)
        {
            for(int i = 0; i <  mChannelControllerList.size(); i++)
            {
                mChannelControllerList.valueAt(i).close();
            }
            mChannelControllerList.clear();
            stopSelf();
        }
        
    }

    AntChannel acquireChannel() throws ChannelNotAvailableException
    {
        AntChannel mAntChannel = null;
        if(null != mAntChannelProvider)
        {
            try
            {
                mAntChannel = mAntChannelProvider.acquireChannel(this, PredefinedNetwork.PUBLIC);
            } catch (RemoteException e)
            {
                die("ACP Remote Ex");
            }
        }        
        return mAntChannel;
    }
    
    public ChannelInfo createNewChannel(final boolean isMaster) throws ChannelNotAvailableException
    {
        ChannelController channelController = null;
      
        synchronized(mCreateChannel_LOCK)
        {
            if(mChannelControllerList.size() == 0)
            {
                Intent i = new Intent(this, this.getClass());
                startService(i);
            }
            AntChannel antChannel = acquireChannel();
            
            if(null != antChannel)
            {

                channelDeviceIdCounter += 1;

                channelController = new ChannelController(antChannel, isMaster, channelDeviceIdCounter, 
                        new ChannelBroadcastListener()
                {                        
                    @Override
                    public void onBroadcastChanged(ChannelInfo newInfo)
                    {
                        mListener.onChannelChanged(newInfo);
                    }
                });

                mChannelControllerList.put(channelDeviceIdCounter, channelController);
            }
        }
        
        if(null == channelController) return null;
        
        return channelController.getCurrentInfo();
    }

    @Override
    public IBinder onBind(Intent arg0)
    {
        return new ChannelServiceComm();
    }
    
    private final BroadcastReceiver mChannelProviderStateChangedReceiver = new BroadcastReceiver()
    {
        @Override
        public void onReceive(Context context, Intent intent)
        {
            if(AntChannelProvider.ACTION_CHANNEL_PROVIDER_STATE_CHANGED.equals(intent.getAction())) {
                boolean update = false;
                int numChannels = intent.getIntExtra(AntChannelProvider.NUM_CHANNELS_AVAILABLE, 0);
                
                if(mChannelAvailable) {
                    // Was a channel available
                    if(0 == numChannels) {
                        // not any more
                        mChannelAvailable = false;
                        update = true;
                    }
                } else {
                    // No channels were available
                    if(numChannels > 0) {
                        // now there are
                        mChannelAvailable = true;
                        update = true;
                    }
                }
                
                if(update && (null != mListener)) {
                    mListener.onChannelAvailable(mChannelAvailable);
                }
            }
        }
    };
    
    private void doBindAntRadioService()
    {
        if(BuildConfig.DEBUG) Log.v(TAG, "doBindAntRadioService");
        
     // Start listing for channel available intents
        registerReceiver(mChannelProviderStateChangedReceiver, new IntentFilter(AntChannelProvider.ACTION_CHANNEL_PROVIDER_STATE_CHANGED));
        
        mAntRadioServiceBound = AntService.bindService(this, mAntRadioServiceConnection);
    }
    
    private void doUnbindAntRadioService()
    {
        if(BuildConfig.DEBUG) Log.v(TAG, "doUnbindAntRadioService");
        
     // Stop listing for channel available intents
        try{
            unregisterReceiver(mChannelProviderStateChangedReceiver);
        } catch (IllegalArgumentException exception) {
            if(BuildConfig.DEBUG) Log.d(TAG, "Attempting to unregister a never registered Channel Provider State Changed receiver.");
        }
        
        if(mAntRadioServiceBound)
        {
            try
            {
                unbindService(mAntRadioServiceConnection);
            }
            catch(IllegalArgumentException e)
            {
                // Not bound, that's what we want anyway
            }

            mAntRadioServiceBound = false;
        }
    }

    @Override
    public void onCreate()
    {
        super.onCreate();
        
        mAntRadioServiceBound = false;
        
        doBindAntRadioService();
    }
    
    @Override
    public void onDestroy()
    {
        closeAllChannels();

        doUnbindAntRadioService();
        mAntChannelProvider = null;
        
        super.onDestroy();
    }

    static void die(String error)
    {
        Log.e(TAG, "DIE: "+ error);
    }
    
}
