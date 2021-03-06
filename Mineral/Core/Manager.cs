﻿using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Backup;
using Mineral.Common.Overlay.Client;
using Mineral.Common.Overlay.Discover;
using Mineral.Common.Overlay.Discover.Node;
using Mineral.Common.Overlay.Messages;
using Mineral.Common.Overlay.Server;
using Mineral.Core.Database;
using Mineral.Core.Database.Fast;
using Mineral.Core.Database.Fast.Callback;
using Mineral.Core.Database.Fast.Callback.StoreTrie;
using Mineral.Core.Net;
using Mineral.Core.Net.MessageHandler;
using Mineral.Core.Net.Peer;
using Mineral.Core.Net.Service;

namespace Mineral.Core
{
    public class Manager
    {
        #region Field
        private static Manager instance = null;

        private DatabaseManager db_manager = new DatabaseManager();
        private TrieService trie_service = new TrieService();

        private NodeManager node_manager = new NodeManager();
        private ChannelManager channel_manager = new ChannelManager();
        private BackupManager backup_manager = new BackupManager();
        private SyncPool sync_pool = new SyncPool();
        private FastSyncCallBack fast_sync_callback = new FastSyncCallBack();

        private MineralNetService net_service = new MineralNetService();
        private MineralNetDelegate net_delegate = new MineralNetDelegate();
        private DiscoverServer discover_server = new DiscoverServer();
        private PeerServer peer_server = new PeerServer();
        private PeerClient peer_client = new PeerClient();
        private PeerStatusCheck peer_check = new PeerStatusCheck();
        private BackupServer backup_server = new BackupServer();
        private AdvanceService advance_service = new AdvanceService();
        private SyncService sync_service = new SyncService();
        private WitnessProductBlockService witness_block_service = new WitnessProductBlockService();
        private FastForward fast_forward = new FastForward();
        private WireTrafficStats traffic_stats = new WireTrafficStats();

        private BlockMessageHandler block_handler = new BlockMessageHandler();
        private ChainInventoryMessageHandler chain_inventory_handler = new ChainInventoryMessageHandler();
        private FetchInventoryDataMessageHandler fetch_inventory_handler = new FetchInventoryDataMessageHandler();
        private InventoryMessageHandler inventory_handler = new InventoryMessageHandler();
        private SyncBlockChainMessageHandler sync_block_handler = new SyncBlockChainMessageHandler();
        private TransactionMessageHandler transaction_handler = new TransactionMessageHandler();
        #endregion


        #region Property
        public static Manager Instance
        {
            get { return instance = instance ?? new Manager(); }
        }


        public DatabaseManager DBManager
        {
            get { return this.db_manager; }
        }

        public TrieService TrieService
        {
            get { return this.trie_service; }
        }

        public NodeManager NodeManager
        {
            get { return this.node_manager; }
        }

        public ChannelManager ChannelManager
        {
            get { return this.channel_manager; }
        }

        public BackupManager BackupManager
        {
            get { return this.backup_manager; }
        }

        public SyncPool SyncPool
        {
            get { return this.sync_pool; }
        }

        public FastSyncCallBack FastSyncCallback
        {
            get { return this.fast_sync_callback; }
        }

        public MineralNetService NetService
        {
            get { return this.net_service; }
        }

        public MineralNetDelegate NetDelegate
        {
            get { return this.net_delegate; }
        }

        public PeerServer PeerServer
        {
            get { return this.peer_server; }
        }

        public PeerClient PeerClient
        {
            get { return this.peer_client; }
        }

        public PeerStatusCheck PeerStatusCheck
        {
            get { return this.peer_check; }
        }

        public BackupServer BackupServer
        {
            get { return this.backup_server; }
        }

        public AdvanceService AdvanceService
        {
            get { return this.advance_service; }
        }

        public SyncService SyncService
        {
            get { return this.sync_service; }
        }

        public WitnessProductBlockService WitnessBlockService
        {
            get { return this.witness_block_service; }
        }

        public FastForward FastForward
        {
            get { return this.fast_forward; }
        }

        public WireTrafficStats TrafficStats
        {
            get { return this.traffic_stats; }
        }

        public BlockMessageHandler BlockHandler
        {
            get { return this.block_handler; }
        }

        public ChainInventoryMessageHandler ChainInventoryHandler
        {
            get { return this.chain_inventory_handler; }
        }

        public FetchInventoryDataMessageHandler FetchInventoryHandler
        {
            get { return this.fetch_inventory_handler; }
        }

        public InventoryMessageHandler InventoryHandler
        {
            get { return this.inventory_handler; }
        }

        public SyncBlockChainMessageHandler SyncBlockHandler
        {
            get { return this.sync_block_handler; }
        }

        public TransactionMessageHandler TransactionHandler
        {
            get { return this.transaction_handler; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        public void Init()
        {
            this.db_manager.Init();
            this.discover_server.Init();
        }
        #endregion
    }
}
