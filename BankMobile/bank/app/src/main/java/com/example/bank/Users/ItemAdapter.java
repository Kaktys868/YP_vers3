package com.example.bank.Users;

import android.content.Context;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.TextView;

import androidx.annotation.NonNull;
import androidx.recyclerview.widget.RecyclerView;

import com.example.bank.Models.UsersModel;
import com.example.bank.R;

import java.util.ArrayList;

public class ItemAdapter  extends RecyclerView.Adapter<ItemAdapter.ViewHolder> {
    private LayoutInflater inflater;
    private ArrayList<UsersModel> users;

    public class ViewHolder extends RecyclerView.ViewHolder{

        public TextView tvProfileName,tvEmail;

        public ViewHolder(@NonNull View itemView) {
            super(itemView);
            tvProfileName = itemView.findViewById(R.id.userNameText);
            tvEmail = itemView.findViewById(R.id.userEmailText);

        }
    }
    public ItemAdapter(Context context, ArrayList<UsersModel> users){
        this.inflater = LayoutInflater.from(context);
        this.users = users;
    }
    @NonNull
    @Override
    public ItemAdapter.ViewHolder onCreateViewHolder(@NonNull ViewGroup parent, int viewType) {
        View view = inflater.inflate(R.layout.activity_profile,parent,false);
        return new ItemAdapter.ViewHolder(view);
    }

    @Override
    public void onBindViewHolder(@NonNull ItemAdapter.ViewHolder holder, int position) {
        UsersModel User = users.get(position);
        holder.tvProfileName.setText(User.Name);
        holder.tvEmail.setText(User.Email);

    }

    @Override
    public int getItemCount() {
        return users.size();
    }
}
