using System;
using System.Collections.Generic;
using System.Text;
using Mineral.Common.Backup;
using Mineral.Common.Overlay.Client;
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
        private AccountStateStoreTrie account_state_store_trie = new AccountStateStoreTrie();

        private NodeManager node_manager = new NodeManager();
        private ChannelManager channel_manager = new ChannelManager();
        private BackupManager backup_manager = new BackupManager();
        private SyncPool sync_pool = new SyncPool();
        private FastSyncCallBack fast_sync_callback = new FastSyncCallBack();

        private MineralNetService net_service = new MineralNetService();
        private MineralNetDelegate net_delegate = new MineralNetDelegate();
        private PeerServer peer_server = new PeerServer();
        private PeerClient peer_client = new PeerClient();
        private PeerStatusCheck peer_check = new PeerStatusCheck();
        private PeerConnection peer_connection = new PeerConnection();
        private BackupServer backup_server = new BackupServer();
        private AdvanceService advance_service = new AdvanceService();
        private SyncService sync_service = new SyncService();
        private WitnessProductBlockService witness_block_service = new WitnessProductBlockService();
        private FastForward fast_forward = new FastForward();
        private WireTrafficStats traffic_stats = new WireTrafficStats();

        private MineralNetHandler net_handler = new MineralNetHandler();
        private P2pHandler p2p_handler = new P2pHandler();
        private BlockMessageHandler block_handler = new BlockMessageHandler();
        private ChainInventoryMessageHandler chain_inventory_handler = new ChainInventoryMessageHandler();
        private FetchInventoryDataMessageHandler fetch_inventory_handler = new FetchInventoryDataMessageHandler();
        private InventoryMessageHandler inventory_handler = new InventoryMessageHandler();
        private SyncBlockChainMessageHandler sync_block_handler = new SyncBlockChainMessageHandler();
        private TransactionMessageHandler transaction_handler = new TransactionMessageHandler();

        private MessageQueue message_queue = new MessageQueue();
        private MessageCodec message_codec = new MessageCodec();
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

        public AccountStateStoreTrie AccountStateTrie
        {
            get { return this.account_state_store_trie; }
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

        public PeerConnection PeerConnection
        {
            get { return this.peer_connection; }
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

        public MineralNetHandler NetHandler
        {
            get { return this.net_handler; }
        }

        public P2pHandler P2pHandler
        {
            get { return this.p2p_handler; }
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

        public MessageQueue MessageQueue
        {
            get { return this.message_queue; }
        }

        public MessageCodec MessageCodec
        {
            get { return this.message_codec; }
        }
        #endregion


        #region Contructor
        #endregion


        #region Event Method
        #endregion


        #region Internal Method
        #endregion


        #region External Method
        #endregion
    }
}
