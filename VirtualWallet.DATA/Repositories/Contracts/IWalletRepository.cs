﻿using VirtualWallet.DATA.Models;

namespace VirtualWallet.DATA.Repositories.Contracts
{
    public interface IWalletRepository
    {
        Task<Wallet?> GetWalletByIdAsync(int id);
        Task<IEnumerable<Wallet>> GetWalletsByUserIdAsync(int userId);
        Task<Wallet> GetWalletByNameAsync(string walletName);
        Task<int> AddWalletAsync(Wallet wallet);
        Task UpdateWalletAsync(Wallet wallet);
        Task RemoveWalletAsync(int walletId);
        Task<Wallet> GetWalletByUserDetailsAsync(string details);
        Task AddUserToJointWalletAsync(int walletId, int userId);
        Task RemoveUserFromJointWalletAsync(int walletId, int userId);
        Task<ICollection<WalletTransaction>> FilterByAsync(WalletTransactionQueryParameters filterParameters, int? userId = null);

        Task<int> GetTotalCountAsync(WalletTransactionQueryParameters filterParameters, int? userId = null);
    }
}
